using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

using CliWrap;
using CliWrap.EventStream;

using Dasync.Collections;

using ICSharpCode.AvalonEdit.Rendering;

using MCT_Windows.Windows;

using MifareWindowsTool.Common;
using MifareWindowsTool.Properties;

namespace MCT_Windows
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public bool ForceReReadTag { get; set; } = false;
        public bool DisplayLog { get; set; } = true;
        public readonly string ACSDriversPage = "https://www.acs.com.hk/en/driver/3/acr122u-usb-nfc-reader/";
        private readonly string Github_MWT_WikiPage = "https://github.com/xavave/Mifare-Windows-Tool/wiki";
        public bool TagFound = false;
        public bool DumpFound = false;
        public bool ScanTagRunning = false;
        //CommandTask<CommandResult> task = null;
        string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public string MainTitle { get; set; } = $"Mifare Windows Tool";
        Tools Tools { get; }
        public List<MCTFile> SelectedKeys = new List<MCTFile>();
        Uri BaseUri = null;
        int cptFail = 0;
        public bool EasyMode
        {
            get => easyMode; set
            {
                SetField(ref easyMode, value);
            }
        }
        IObservable<long> ObservableScan = null;
        public CancellationTokenSource ScanCTS = new CancellationTokenSource();
        public CancellationTokenSource ProcessCTS = new CancellationTokenSource();
        private bool easyMode;


        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string? propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }


        public MainWindow()
        {
            this.DataContext = this;
            InitializeComponent();
            EasyMode = false;
            Uri iconUri = new Uri("pack://application:,,,/Resources/MWT.ico", UriKind.RelativeOrAbsolute);
            BaseUri = BaseUriHelper.GetBaseUri(this);
            this.Icon = BitmapFrame.Create(iconUri);
            MainTitle += $" v{version}";
            this.Title = $"{MainTitle}";
            btnPauseDisplay.Content = $"{Translate.Key(nameof(MifareWindowsTool.Properties.Resources.PauseOff))}";
            //ShowPauseButton(task != null);
            Tools = new Tools();
            var newVersion = Tools.CheckNewVersion();
            if (newVersion != null)
            {
                var comp = Assembly.GetExecutingAssembly().GetName().Version.CompareTo(newVersion);
                if (comp < 0)
                {
                    var ret = MessageBox.Show(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.NewerVersionExists)), "Version", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (ret == MessageBoxResult.Yes)
                    {
                        Process.Start("https://github.com/xavave/Mifare-Windows-Tool/releases/latest");
                    }
                }

            }
            var defaultPath = Tools.GetSetting("DefaultDumpPath");
            if (!string.IsNullOrWhiteSpace(defaultPath) && Directory.Exists(defaultPath))
                DumpBase.DefaultDumpPath = defaultPath;
            else
                DumpBase.DefaultDumpPath = Path.Combine(DumpBase.DefaultWorkingDir, "dumps");

            var defaultKeysPath = Tools.GetSetting("DefaultKeysPath");
            if (!string.IsNullOrWhiteSpace(defaultKeysPath) && Directory.Exists(defaultKeysPath))
                DumpBase.DefaultKeysPath = defaultKeysPath;
            else
                DumpBase.DefaultKeysPath = Path.Combine(DumpBase.DefaultWorkingDir, "keys");

            var ofd = DumpBase.CreateOpenDialog();
            //ofd.InitialDirectory = DumpBase.DefaultDumpPath;

            if (!Tools.TestWritePermission(ofd.InitialDirectory))
            {
                MessageBox.Show(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.PleaseRestartAsAdmin)));
                Application.Current.Shutdown();
            }
            if (CheckSetDriver()) PeriodicScanTag();
        }
        private bool CheckSetDriver()
        {
            try
            {
                var acrState = Tools.DriverState("ACR122U");

                if (acrState != "")
                {
                    var libusbkState = Tools.DriverState("LibUsbk");
                    if (acrState != "running" && libusbkState == "stopped")
                    {
                        MessageBox.Show(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.BadgeReaderAcr122NotFound)), "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    if (libusbkState != "running")
                    {
                        var dr = MessageBox.Show(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.DriverLibUsbKNonInstalled)), "LibUsbK", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                        if (dr == MessageBoxResult.OK)
                        {
                            return Tools.InstallLibUsbKDriver();
                        }
                    }
                    else
                        return true;
                }
                else
                {

                    var dr = MessageBox.Show(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.DriverACR122NotInstalled)), "ACR122U", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (dr == MessageBoxResult.OK)
                    {
                        Process.Start(ACSDriversPage);

                    }
                }
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }
        public void PeriodicScanTag()
        {
            if (ScanTagRunning) return;

            ObservableScan = Observable.Interval(TimeSpan.FromSeconds(3));
            // Token for cancelation
            ScanCTS = new CancellationTokenSource();
            // Subscribe the observable to the task on execution.
            ObservableScan.Subscribe(x =>
            {
                Application.Current?.Dispatcher.Invoke(async () =>
                {
                    ScanCTS.Token.Register(() => ScanTagRunning = false);
                    if (!ckEnablePeriodicTagScan.IsChecked.HasValue || ckEnablePeriodicTagScan.IsChecked.Value == false) return;
                    if (DisplayLog) editor.AppendText($"{DateTime.Now} -  {Translate.Key(nameof(MifareWindowsTool.Properties.Resources.AutoScanTagRunning))}...\n");
                    await RunNfcListAsync();
                });
            }, ScanCTS.Token);
            ScanTagRunning = true;
        }

        private async void btnReadTag_Click(object sender, RoutedEventArgs e)
        {
            await ReadTagAsync(TagAction.ReadSource);

        }
        private async void btnWriteTag_Click(object sender, RoutedEventArgs e)
        {
            var writeAfter = await ReadTagAsync(TagAction.ReadTarget);
            if (writeAfter)
            {
                OpenWriteDumpWindow();
            }
        }

        private async Task<bool> ReadTagAsync(TagAction act)
        {
            if (Tools.running) return false;
            //DumpFound = false;
            if (!TagFound && !ScanTagRunning)
                PeriodicScanTag();

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                if (!ckEnablePeriodicTagScan.IsChecked.HasValue || !ckEnablePeriodicTagScan.IsChecked.Value)
                {
                    await RunNfcListAsync();
                }

                if (!string.IsNullOrWhiteSpace(DumpBase.CurrentUID))
                {
                    DumpFound = ReadDump(act);
                }

                if (TagFound)
                {
                    StopScanTag();
                    if (act == TagAction.ReadSource)
                    {
                        if (!DumpFound)
                        {
                            MapKeyToSectorWindow mtsWin = new MapKeyToSectorWindow(this, Tools, Translate.Key(nameof(MifareWindowsTool.Properties.Resources.UsedForSourceMapping)), Translate.Key(nameof(MifareWindowsTool.Properties.Resources.Source)));
                            Mouse.OverrideCursor = null;
                            var ret = mtsWin.ShowDialog();
                            if (ret.HasValue && ret.Value)
                            {
                                await RunMfocAsync(SelectedKeys, Tools.SourceBinaryDump.DumpFileName, act,
                                    mtsWin.chkCustomProbeNb.IsChecked.HasValue && mtsWin.chkCustomProbeNb.IsChecked.Value ? mtsWin.udNbProbes.Value : 20, mtsWin.chkCustomProbeNb.IsChecked.HasValue && mtsWin.chkCustomProbeNb.IsChecked.Value ? mtsWin.udTolerance.Value : 20);

                            }
                            else
                                PeriodicScanTag();
                        }
                        else
                        {
                            Mouse.OverrideCursor = null;
                            ShowDump(Tools.SourceBinaryDump);
                        }
                    }
                    else if (act == TagAction.ReadTarget)
                    {
                        if (!DumpFound)
                        {
                            Mouse.OverrideCursor = null;
                            if (!ForceReReadTag) MessageBox.Show(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.InfoMessageTagToReadAndDecode)), "Information");
                            MapKeyToSectorWindow mtsWin = new MapKeyToSectorWindow(this, Tools, Translate.Key(nameof(MifareWindowsTool.Properties.Resources.UsedForTargetMapping)), Translate.Key(nameof(MifareWindowsTool.Properties.Resources.Target)));
                            var ret = mtsWin.ShowDialog();
                            if (ret.HasValue && ret.Value)
                            {
                                await RunMfocAsync(SelectedKeys, Tools.TargetBinaryDump.DumpFileName, act,
                                        mtsWin.chkCustomProbeNb.IsChecked.HasValue && mtsWin.chkCustomProbeNb.IsChecked.Value ? mtsWin.udNbProbes.Value : 20, mtsWin.chkCustomProbeNb.IsChecked.HasValue && mtsWin.chkCustomProbeNb.IsChecked.Value ? mtsWin.udTolerance.Value : 20);

                            }
                            else
                                PeriodicScanTag();

                        }
                        else
                        {
                            Mouse.OverrideCursor = null;
                            return true;
                        }

                    }

                }
                else
                {
                    LogAppend(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.NoTagDetectedOnReader)));
                }
                return false;

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private bool ReadDump(TagAction act)
        {
            IDump dump = null;
            if (act == TagAction.ReadSource) dump = Tools.SourceBinaryDump;
            else if (act == TagAction.ReadTarget) dump = Tools.TargetBinaryDump;
            if (dump == null) return false;
            dump.StrDumpUID = DumpBase.CurrentUID;
            if (dump.DumpFileFullName == null) dump.DumpFileFullName = System.IO.Path.Combine(DumpBase.DefaultDumpPath, $"Dump_{DumpBase.CurrentUID}.mfd");
            Tools.TMPFILE_UNK = $"mfc_{dump.StrDumpUID}_unknownMfocSectorInfo.txt";
            Tools.TMPFILE_FND = $"mfc_{dump.StrDumpUID}_foundKeys.txt";
            if (Tools.CheckAndUseDumpIfExists(dump, act, out bool forceReRead, EasyMode))
            {
                ForceReReadTag = forceReRead;
                GetDumpBinaryOutput(dump.DumpFileFullName, act);
                return true;
            }
            else
            {
                ForceReReadTag = forceReRead;
                return false;
            }
        }

        private void OpenWriteDumpWindow()
        {
            WriteDumpWindow wdw = new WriteDumpWindow(this, Tools);
            var ret = wdw.ShowDialog();
            if (!ret.HasValue || !ret.Value)
                PeriodicScanTag();
        }

        public void ValidateActions(bool enable)
        {

            btnReadTag.IsEnabled = enable;
            btnWriteTag.IsEnabled = enable;
            ckEnablePeriodicTagScan.IsEnabled = enable;

        }

        public void ShowAbortButton(bool show = true)
        {
            if (show)
            {
                btnAbortCurrentTask.Content = $"{Translate.Key(nameof(MifareWindowsTool.Properties.Resources.Abort))}";
                btnAbortCurrentTask.Visibility = Visibility.Visible;
            }
            else
                btnAbortCurrentTask.Visibility = Visibility.Hidden;

        }
        public void ShowPauseButton(bool? display = true)
        {

            if (display.HasValue)
            {
                DisplayLog = display.Value;
                if (display.Value)
                {
                    btnPauseDisplay.Content = $"{Translate.Key(nameof(MifareWindowsTool.Properties.Resources.PauseOff))}";
                    btnPauseDisplay.Visibility = Visibility.Visible;
                }
                else if (!display.Value)
                {
                    btnPauseDisplay.Content = $"{Translate.Key(nameof(MifareWindowsTool.Properties.Resources.PauseOn))}";
                }
            }
            else
            {
                DisplayLog = true;
                btnPauseDisplay.Visibility = Visibility.Hidden;
            }


        }

        public void ShowDump(IDump dump)
        {
            ShowDump(dump.DumpFileFullName);
        }
        public void ShowDump(string fileName)
        {
            Dispatcher.Invoke(() =>
            {
                DumpWindow dw = new DumpWindow(fileName);
                var dr = dw.ShowDialog();
                if (dr.HasValue && dr.Value)
                    PeriodicScanTag();
            });
        }
        //public static void DoEvents()
        //{
        //    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
        //                                          new Action(delegate { }));
        //}

        public async Task<bool> RunPcscScanACR122()
        {

            using (var cts = new CancellationTokenSource())
            {
                bool IsACR122 = false;
                ProcessCTS = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(10)); // e.g. timeout of 5 seconds
                var cmd = Cli.Wrap("nfctools\\pcsc_scan.exe");


                await foreach (CommandEvent cmdEvent in cmd.ListenAsync(cts.Token))
                {
                    switch (cmdEvent)
                    {
                        case StandardOutputCommandEvent stdOut:
                            LogAppend(stdOut.Text);
                            if (stdOut.Text.Contains("ACR122")) IsACR122 = true;
                            break;
                        case StandardErrorCommandEvent stdErr:
                            ErrorAppend(stdErr.Text);
                            break;
                        default: break;
                    }
                }

                return IsACR122;
            }
        }

        public async Task<string> RunNfcListAsync()
        {

            try
            {
                var stdOutFull = "";
                var cmd = Cli.Wrap("nfctools\\nfc-list.exe").WithValidation(CommandResultValidation.None);
                //task = cmd.ExecuteAsync();
                //ShowPauseButton(task != null);
                await foreach (CommandEvent cmdEvent in cmd.ListenAsync(ScanCTS.Token))
                {
                    switch (cmdEvent)
                    {
                        case StandardOutputCommandEvent stdOut:
                            stdOutFull += stdOut.Text;
                            LogAppend(stdOut.Text);
                            if (stdOut.Text.Contains("UID"))
                            {
                                DumpBase.CurrentUID = SetCurrentUID(stdOutFull);
                            }

                            break;
                        case StandardErrorCommandEvent stdErr:
                            ErrorAppend(stdErr.Text);
                            break;
                        case ExitedCommandEvent exited:
                            if (exited.ExitCode != 0)
                            {
                                if (cptFail < 1)
                                {
                                    var dr = MessageBox.Show(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.DriverLibUsbKNonInstalled)), "LibUsbK", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                                    if (dr == MessageBoxResult.OK)
                                    {
                                        if (Tools.InstallLibUsbKDriver())
                                            cptFail++;
                                    }
                                }
                                DumpBase.CurrentUID = "";
                            }
                            else if (stdOutFull.Contains("No NFC device found."))
                            {

                                ScanTagRunning = false;
                                ScanCTS.Cancel();
                            }


                            break;
                        default: break;
                    }
                }
                return DumpBase.CurrentUID;
            }
            catch (TaskCanceledException)
            {
                ckEnablePeriodicTagScan.IsChecked = false;
                ScanTagRunning = false;
                return "";
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return "";
            }
        }

        private string SetCurrentUID(string text)
        {
            var newUID = "";

            var retUID = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Where(t => t.Contains("UID")).LastOrDefault();
            if (retUID != null && retUID.Contains(": "))
            {
                newUID = retUID.Substring(retUID.LastIndexOf(": ") + ": ".Length).Replace(" ", "").ToUpper();
                if (DumpBase.CurrentUID != newUID)
                {
                    DumpBase.CurrentUID = newUID;
                    this.Title = $"{MainTitle}: {Translate.Key(nameof(MifareWindowsTool.Properties.Resources.NewUIDFound))}: {DumpBase.CurrentUID}";

                    Tools.PlayBeep(BaseUri);

                    TagFound = true;
                }
            }

            else
            {
                DumpBase.CurrentUID = "";
                TagFound = false;
                this.Title = $"{MainTitle}: {Translate.Key(nameof(MifareWindowsTool.Properties.Resources.NoTag))}";
            }

            return newUID;
        }

        public void StopScanTag()
        {
            if (ScanTagRunning)
            {

                ScanTagRunning = false;
                if (ScanCTS != null) ScanCTS.Cancel();
                if (ckEnablePeriodicTagScan.IsChecked.Value)
                    LogAppend(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.AutoScanTagStopped)));
            }
        }
        public async Task RunMifareClassicFormatAsync(bool withoutValidation = false)
        {
            StopScanTag();
            ValidateActions(false);
            ShowAbortButton();
            ShowPauseButton();
            try
            {
                string args = "";
                if (System.IO.File.Exists(Tools.TargetBinaryDump.DumpFileFullName)) args += $" \"{Tools.TargetBinaryDump.DumpFileFullName}\"";
                ProcessCTS = new CancellationTokenSource();
                var arguments = $"-y {args}";
                LogAppend($"mifare-classic-format {arguments}");
                var cmd = Cli.Wrap("nfctools\\mifare-classic-format.exe").WithArguments(arguments).WithValidation(withoutValidation ? CommandResultValidation.None : CommandResultValidation.ZeroExitCode);
                //task = cmd.ExecuteAsync();
                //ShowPauseButton(task != null);
                await foreach (CommandEvent cmdEvent in cmd.ListenAsync(ProcessCTS.Token))
                {
                    switch (cmdEvent)
                    {
                        case StandardOutputCommandEvent stdOut:
                            LogAppend(stdOut.Text);
                            break;
                        case StandardErrorCommandEvent stdErr:
                            ErrorAppend(stdErr.Text);
                            break;
                        default: break;
                    }
                }

            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                //ProcessCTS.Dispose();
                ValidateActions(true);
                ShowAbortButton(false);
                ShowPauseButton(null);
            }

        }
        //[DllImport(@"C:\work\libnfc_test\build\utils\Debug\nfc-list.exe")]
        //public static extern int nfclist();
        public async Task RunNfcMfclassicAsync(TagAction act, bool bWriteBlock0, bool useKeyA, bool haltOnError, TagType tagType)
        {
            try
            {
                StopScanTag();
                ValidateActions(false);
                ShowAbortButton();
                var sourceDump = Tools.SourceBinaryDump;
                var targetDump = Tools.TargetBinaryDump;
                char writeMode = bWriteBlock0 == true ? 'W' : 'w';
                char useKey = useKeyA == true ? 'A' : 'B';
                char cHaltOnError = haltOnError == true ? useKey = char.ToLower(useKey) : char.ToUpper(useKey);
                //if (tagType == TagType.UnlockedGen1) writeMode = 'W'; else if (tagType == TagType.DirectCUIDgen2) writeMode = 'C';
                ProcessCTS = new CancellationTokenSource();
                //var arguments = $"{writeMode} {cHaltOnError} U{targetDump.StrDumpUID} \"{sourceDump.DumpFileFullName}\" \"{targetDump.DumpFileFullName}\"";
                var arguments = $"{writeMode} {cHaltOnError} \"{sourceDump.DumpFileFullName}\" \"{targetDump.DumpFileFullName}\"";
                LogAppend($"nfc-mfclassic {arguments}");

                var cmd = Cli.Wrap("nfctools\\nfc-mfclassic.exe").WithArguments(arguments).WithValidation(CommandResultValidation.None)
                    .WithStandardOutputPipe(PipeTarget.ToDelegate(LogAppend))
                    .WithStandardErrorPipe(PipeTarget.ToDelegate(ErrorAppend));
                //task = cmd.ExecuteAsync();
                //ShowPauseButton(task != null);
                var result = await cmd.ExecuteAsync(ProcessCTS.Token);
                MessageBox.Show(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.Finished)));

            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //ProcessCTS.Dispose();
                ValidateActions(true);
                ShowAbortButton(false);
                ShowPauseButton(null);
            }

        }
        public async Task RunMfocAsync(List<MCTFile> keys, string tmpFileMfd, TagAction act, int? nbProbes = 20, int? tolerance = 20)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                bool showDump = false;
                StopScanTag();
                ValidateActions(false);
                ShowAbortButton();
                string arguments = "";
                tmpFileMfd = Path.Combine(DumpBase.DefaultDumpPath, tmpFileMfd);

                var tmpFileUnk = Path.Combine(DumpBase.DefaultDumpPath, Tools.TMPFILE_UNK);

                if (System.IO.File.Exists(tmpFileUnk))
                    arguments += $" -D \"{tmpFileUnk}\"";

                arguments += $"-P {nbProbes} -T {tolerance} ";

                foreach (var key in keys.Select(k => k.FileName))
                {
                    arguments += $" -f \"{Path.Combine(DumpBase.DefaultKeysPath, key)}\"";
                }
                arguments += $" -O\"{tmpFileMfd}\"";
                ProcessCTS = new CancellationTokenSource();
                LogAppend($"mfoc-hardnested {arguments}");
                var cmd = Cli.Wrap("nfctools\\mfoc-hardnested.exe").WithArguments(arguments).WithWorkingDirectory(DumpBase.DefaultWorkingDir).WithValidation(CommandResultValidation.None);
                //task = cmd.ExecuteAsync();
                //ShowPauseButton(task != null);
                await foreach (CommandEvent cmdEvent in cmd.ListenAsync(ProcessCTS.Token))
                {
                    switch (cmdEvent)
                    {

                        case StandardOutputCommandEvent stdOut:
                            LogAppend(stdOut.Text);
                            if (act == TagAction.ReadSource)
                                if (stdOut.Text.Contains($"dumping keys to a file"))
                                {
                                    showDump = true;

                                }
                                else if (act == TagAction.ReadTarget)
                                    if (stdOut.Text.Contains($"dumping"))
                                    {
                                        OpenWriteDumpWindow();

                                    }
                            break;
                        case StandardErrorCommandEvent stdErr:
                            ErrorAppend(stdErr.Text);
                            break;
                        case ExitedCommandEvent exited:
                            if (showDump) ShowDump(Tools.SourceBinaryDump);
                            GetDumpBinaryOutput(tmpFileMfd, act);
                            break;
                    }
                }


            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                PeriodicScanTag();
            }
            finally
            {
                ValidateActions(true);
                ShowAbortButton(false);
                ShowPauseButton(null);
                Mouse.OverrideCursor = null;
                //ProcessCTS.Dispose();
            }

        }

        private void GetDumpBinaryOutput(string tmpFileMfd, TagAction act)
        {
            if (!File.Exists(tmpFileMfd)) return;
            var binary = File.ReadAllBytes(tmpFileMfd);
            if (act == TagAction.ReadSource && (Tools.SourceBinaryDump.DumpData.HexData == null || !Tools.SourceBinaryDump.DumpData.HexData.SequenceEqual(binary)))
                Tools.SourceBinaryDump.DumpData.HexData = binary;
            else if (act == TagAction.ReadTarget && (Tools.TargetBinaryDump.DumpData.HexData == null || !Tools.TargetBinaryDump.DumpData.HexData.SequenceEqual(binary)))
                Tools.TargetBinaryDump.DumpData.HexData = binary;
        }

        public void ErrorAppend(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                if (DisplayLog) ColorizeLine(msg, Brushes.Orange);
            }, DispatcherPriority.Normal);
        }

        private void ColorizeLine(string msg, Brush brush)
        {
            editor.AppendText(msg + Environment.NewLine);
            var lineNumber = editor.Document.LineCount - 1;
            var line = editor.Document.GetLineByNumber(lineNumber);
            editor.Select(line.Offset, line.Length);
            editor.TextArea.TextView.LineTransformers.Add(new LineColorizer(lineNumber, brush));
        }

        public void LogAppend(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                if (DisplayLog) ColorizeLine(msg, Brushes.Lime);
            }, DispatcherPriority.Normal);
        }

        private void btnEditAddKeyFile_Click(object sender, RoutedEventArgs e)
        {
            StopScanTag();
            SelectKeyFilesWindow ekf = new SelectKeyFilesWindow(this, Tools);
            ekf.ShowDialog();
            PeriodicScanTag();
        }



        private void btnEditDumpFile_Click(object sender, RoutedEventArgs e)
        {
            StopScanTag();
            var ofd = DumpBase.CreateOpenDialog();
            var dr = ofd.ShowDialog();
            if (dr.Value)
            {
                ShowDump(ofd.FileName);
            }
            else
                PeriodicScanTag();

        }

        private void btnInfos_Click(object sender, RoutedEventArgs e)
        {

            WinInfo winInfo = new WinInfo();
            winInfo.Show();

            Process.Start(Github_MWT_WikiPage);
        }

        private void btnTools_Click(object sender, RoutedEventArgs e)
        {
            var stw = new SelectToolWindow(this, Tools);
            stw.Show();
        }


        private void btnAbortCurrentTask_Click(object sender, RoutedEventArgs e)
        {

            ProcessCTS.Cancel();
            ShowAbortButton(false);
            ValidateActions(true);
        }
        private void ckEnablePeriodicTagScan_Checked(object sender, RoutedEventArgs e)
        {

            if (ckEnablePeriodicTagScan.IsChecked.HasValue && ckEnablePeriodicTagScan.IsChecked.Value)
                PeriodicScanTag();
        }

        private void editor_TextChanged(object sender, EventArgs e)
        {
            (sender as ICSharpCode.AvalonEdit.TextEditor).ScrollToEnd();
        }

        private void btnPauseDisplay_Click(object sender, RoutedEventArgs e)
        {
            ShowPauseButton(!DisplayLog);

        }
    }
    public class LineColorizer : DocumentColorizingTransformer
    {
        int lineNumber;
        System.Windows.Media.Brush brush;

        public LineColorizer(int lineNumber, System.Windows.Media.Brush brush)
        {
            this.lineNumber = lineNumber;
            this.brush = brush;
        }

        protected override void ColorizeLine(ICSharpCode.AvalonEdit.Document.DocumentLine line)
        {
            if (!line.IsDeleted && line.LineNumber == lineNumber)
            {
                ChangeLinePart(line.Offset, line.EndOffset, ApplyChanges);
            }
        }

        void ApplyChanges(VisualLineElement element)
        {
            // This is where you do anything with the line
            element.TextRunProperties.SetForegroundBrush(brush);
        }
    }
}
