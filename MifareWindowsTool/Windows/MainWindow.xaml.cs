using CliWrap;
using MCT_Windows.Windows;

using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace MCT_Windows
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool TagFound = false;
        public bool DumpFound = false;
        public bool ScanTagRunning = false;
        string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public string MainTitle { get; set; } = $"Mifare Windows Tool";
        Tools t = null;
        OpenFileDialog ofd = new OpenFileDialog();
        public List<File> SelectedKeys = new List<File>();
        Uri BaseUri = null;
        string StdOutNfclist = "";
        string StdOutMfoc = "";
        IObservable<long> ObservableScan = null;
        CancellationTokenSource ScanCTS = null;
        CancellationTokenSource ProcessCTS = null;
        public MainWindow()
        {
            InitializeComponent();
            Uri iconUri = new Uri("pack://application:,,,/Resources/MWT.ico", UriKind.RelativeOrAbsolute);
            BaseUri = BaseUriHelper.GetBaseUri(this);
            this.Icon = BitmapFrame.Create(iconUri);
            MainTitle += $" v{version}";
            this.Title = $"{MainTitle}";
            ofd.Filter = MifareWindowsTool.Properties.Resources.DumpFileFilter;

            t = new Tools(this);

            ofd.InitialDirectory = Path.Combine(t.DefaultWorkingDir, "dumps");

            if (!t.TestWritePermission(ofd.InitialDirectory))
            {
                MessageBox.Show(MifareWindowsTool.Properties.Resources.PleaseRestartAsAdmin);
                Application.Current.Shutdown();
            }
            Task.Run(() => PeriodicScanTag());

        }

        public async Task PeriodicScanTag(int delay = 0)
        {
            if (ScanTagRunning) return;

            await Task.Delay(delay);

            ObservableScan = Observable.Interval(TimeSpan.FromSeconds(3));
            // Token for cancelation
            ScanCTS = new CancellationTokenSource();
            // Subscribe the observable to the task on execution.
            ObservableScan.Subscribe(x =>
            {
                Application.Current.Dispatcher.Invoke(async () =>
                {
                    if (!ckEnablePeriodicTagScan.IsChecked.HasValue || ckEnablePeriodicTagScan.IsChecked.Value == false) return;
                    rtbOutput.Text += $"{DateTime.Now} -  {MifareWindowsTool.Properties.Resources.AutoScanTagRunning}...\n";
                    await RunNfcList();
                });
            }, ScanCTS.Token);
            ScanTagRunning = true;
        }

        private async void btnReadTag_Click(object sender, RoutedEventArgs e)
        {
            await ReadTag(TagAction.ReadSource);

        }
        private async void btnWriteTag_Click(object sender, RoutedEventArgs e)
        {
            await ReadTag(TagAction.ReadTarget);
        }

        private async Task ReadTag(TagAction act)
        {

            if (t.running) return;
            DumpFound = false;
            if (!TagFound && !ScanTagRunning)
                await PeriodicScanTag();

            if (!ckEnablePeriodicTagScan.IsChecked.HasValue || !ckEnablePeriodicTagScan.IsChecked.Value)
            {
                await RunNfcList();
            }

            if (!string.IsNullOrWhiteSpace(t.CurrentUID))
            {
                if (act == TagAction.ReadSource)
                {
                    t.mySourceUID = t.CurrentUID;
                    t.TMPFILE_UNK = $"mfc_{ t.mySourceUID}_unknownMfocSectorInfo.txt";
                    t.TMPFILESOURCE_MFD = $"mfc_{ t.mySourceUID}.dump";
                    t.TMPFILE_FND = $"mfc_{ t.mySourceUID}_foundKeys.txt";
                    if (t.CheckAndUseDumpIfExists(t.TMPFILESOURCE_MFD))
                    {
                        DumpFound = true;

                    }
                }
                else if (act == TagAction.ReadTarget)
                {
                    t.myTargetUID = t.CurrentUID;
                    t.TMPFILE_UNK = $"mfc_{ t.myTargetUID}_unknownMfocSectorInfo.txt";
                    t.TMPFILE_TARGETMFD = $"mfc_{ t.myTargetUID}.dump";
                    t.TMPFILE_FND = $"mfc_{ t.myTargetUID}_foundKeys.txt";
                    if (t.CheckAndUseDumpIfExists(t.TMPFILE_TARGETMFD))
                    {
                        DumpFound = true;
                    }

                }
            }

            if (TagFound)
            {
                StopScanTag();
                if (act == TagAction.ReadSource)
                {
                    if (!DumpFound)
                    {
                        MapKeyToSectorWindow mtsWin = new MapKeyToSectorWindow(this, t, MifareWindowsTool.Properties.Resources.UsedForSourceMapping);
                        var ret = mtsWin.ShowDialog();
                        if (ret.HasValue && ret.Value)
                        {
                            await RunMfoc(SelectedKeys, t.TMPFILESOURCE_MFD, act);
                            if (StdOutMfoc.Contains($"dumping keys to a file"))
                            {
                                ShowDump("dumps\\" + t.TMPFILESOURCE_MFD);
                                StdOutMfoc = string.Empty;
                            }
                        }
                        else
                            await PeriodicScanTag();
                    }
                    else
                    {
                        ShowDump("dumps\\" + t.TMPFILESOURCE_MFD);
                    }
                }
                else if (act == TagAction.ReadTarget)
                {
                    if (!DumpFound)
                    {
                        MapKeyToSectorWindow mtsWin = new MapKeyToSectorWindow(this, t, MifareWindowsTool.Properties.Resources.UsedForTargetMapping);
                        var ret = mtsWin.ShowDialog();
                        if (ret.HasValue && ret.Value)
                        {
                            await RunMfoc(SelectedKeys, t.TMPFILE_TARGETMFD, act);
                            if (StdOutMfoc.Contains($"dumping"))
                            {
                                await OpenWriteDumpWindow();
                                StdOutMfoc = string.Empty;
                            }
                        }
                        else
                            await PeriodicScanTag();

                    }
                    else
                    {
                        await OpenWriteDumpWindow();
                    }

                }

            }
            else
            {
                logAppend(MifareWindowsTool.Properties.Resources.NoTagDetectedOnReader);
            }

        }

        private async Task OpenWriteDumpWindow()
        {
            WriteDumpWindow wdw = new WriteDumpWindow(this, t);
            var ret = wdw.ShowDialog();
            if (!ret.HasValue || !ret.Value)
                await PeriodicScanTag();
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
                btnAbortCurrentTask.Content = $"{MifareWindowsTool.Properties.Resources.Abort}";
                btnAbortCurrentTask.Visibility = Visibility.Visible;
            }
            else
                btnAbortCurrentTask.Visibility = Visibility.Hidden;

        }


        public void ShowDump(string fileName)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                DumpWindow dw = new DumpWindow(t, fileName);
                var dr = dw.ShowDialog();
                if (dr.HasValue && dr.Value)
                    await PeriodicScanTag();
            });

        }

        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                  new Action(delegate { }));
        }

        private async Task RunNfcList()
        {

            var result = await Cli.Wrap(@"nfctools\\nfc-list.exe")
   .SetStandardOutputCallback(l => logAppend(l))
   .SetStandardErrorCallback(l => logAppend(l))
   .ExecuteAsync();

            var exitCode = result.ExitCode;
            StdOutNfclist = result.StandardOutput;
            var stdErr = result.StandardError;
            var startTime = result.StartTime;
            var exitTime = result.ExitTime;
            var runTime = result.RunTime;

            var retUID = StdOutNfclist.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Where(t => t.Contains("UID")).LastOrDefault();
            if (retUID != null && retUID.Contains(": "))
            {
                var newUID = retUID.Substring(retUID.IndexOf(": ") + ": ".Length).Replace(" ", "").ToUpper();
                if (t.CurrentUID != newUID)
                {
                    t.CurrentUID = newUID;
                    this.Title = $"{MainTitle}: {MifareWindowsTool.Properties.Resources.NewUIDFound}: { t.CurrentUID}";

                    t.PlayBeep(BaseUri);

                    TagFound = true;
                }
            }

            else
            {
                t.CurrentUID = "";
                TagFound = false;
                this.Title = $"{MainTitle}: {MifareWindowsTool.Properties.Resources.NoTag}";
            }
        }
        public void StopScanTag()
        {
            if (ScanTagRunning)
            {
                ScanTagRunning = false;
                ScanCTS.Cancel();
                logAppend(MifareWindowsTool.Properties.Resources.AutoScanTagStopped);
            }
        }
        public async Task RunMifareClassicFormat()
        {
            StopScanTag();
            ValidateActions(false);
            ShowAbortButton();
            try
            {
                string args = "";
                if (System.IO.File.Exists("dumps\\" + t.TMPFILE_TARGETMFD))
                    args += $" \"{"dumps\\" + t.TMPFILE_TARGETMFD}\"";
                ProcessCTS = new CancellationTokenSource();
                var result = await Cli.Wrap("nfctools\\mifare-classic-format.exe").SetArguments($"-y {args}")
      .SetStandardOutputCallback(l => logAppend(l))
      .SetStandardErrorCallback(l => logAppend(l))
      .SetCancellationToken(ProcessCTS.Token)
      .ExecuteAsync();

                var exitCode = result.ExitCode;
                var stdOut = result.StandardOutput;
                var stdErr = result.StandardError;
                var startTime = result.StartTime;
                var exitTime = result.ExitTime;
                var runTime = result.RunTime;
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                ProcessCTS.Dispose();
                ValidateActions(true);
                ShowAbortButton(false);
            }



        }
        public async Task RunNfcMfclassic(TagAction act, bool bWriteBlock0, bool useKeyA, bool haltOnError, TagType tagType)
        {
            try
            {
                StopScanTag();
                ValidateActions(false);
                ShowAbortButton();
                var sourceDump = "dumps\\" + t.TMPFILESOURCE_MFD;
                var targetDump = "dumps\\" + t.TMPFILE_TARGETMFD;
                char writeMode = bWriteBlock0 == true ? 'W' : 'w';
                char useKey = useKeyA == true ? 'A' : 'B';
                char cHaltOnError = haltOnError == true ? useKey = char.ToLower(useKey) : char.ToUpper(useKey);
                if (tagType == TagType.UnlockedGen1) writeMode = 'W'; else if (tagType == TagType.DirectCUIDgen2) writeMode = 'C';
                ProcessCTS = new CancellationTokenSource();
                var result = await Cli.Wrap("nfctools\\nfc-mfclassic.exe").SetArguments($"{writeMode} {cHaltOnError} u \"{sourceDump}\" \"{targetDump}\"")
       .SetStandardOutputCallback(l => logAppend(l))
       .SetStandardErrorCallback(l => logAppend(l))
       .SetCancellationToken(ProcessCTS.Token)
       .ExecuteAsync();
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
                ProcessCTS.Dispose();
                ValidateActions(true);
                ShowAbortButton(false);
            }


        }
        public async Task RunMfoc(List<File> keys, string tmpFileMfd, TagAction act)
        {
            try
            {
                StopScanTag();
                ValidateActions(false);
                ShowAbortButton();
                string arguments = "";
                tmpFileMfd = "dumps\\" + tmpFileMfd;

                var tmpFileUnk = "dumps\\" + t.TMPFILE_UNK;

                if (System.IO.File.Exists(tmpFileUnk))
                    arguments += $" -D \"{tmpFileUnk}\"";

                foreach (var key in keys.Select(k => k.FileName))
                {
                    arguments += $" -f \"keys\\{key}\"";

                }
                arguments += $" -O\"{tmpFileMfd}\"";
                ProcessCTS = new CancellationTokenSource();

                var result = await Cli.Wrap("nfctools\\mfoc.exe").SetArguments(arguments).SetWorkingDirectory(t.DefaultWorkingDir)
    .SetStandardOutputCallback(l => logAppend(l))
    .SetStandardErrorCallback(l => logAppend(l))
    .SetCancellationToken(ProcessCTS.Token)
    .ExecuteAsync();

                StdOutMfoc = result.StandardOutput;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                await PeriodicScanTag();
            }
            finally
            {
                ValidateActions(true);
                ShowAbortButton(false);
                ProcessCTS.Dispose();
            }

        }

        public void logAppend(string msg)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                rtbOutput.AppendText(msg + "\n");
            }));
        }

        private async void btnEditAddKeyFile_Click(object sender, RoutedEventArgs e)
        {
            StopScanTag();
            SelectKeyFilesWindow ekf = new SelectKeyFilesWindow(this, t);
            ekf.ShowDialog();
            await PeriodicScanTag();
        }

        private void rtbOutput_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }


        private async void btnEditDumpFile_Click(object sender, RoutedEventArgs e)
        {
            StopScanTag();
            var dr = ofd.ShowDialog();
            if (dr.Value)
            {


                ShowDump(ofd.FileName);
            }
            else
                await PeriodicScanTag();

        }

        private void btnInfos_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/xavave/Mifare-Windows-Tool/wiki");
        }

        private async void btnTools_Click(object sender, RoutedEventArgs e)
        {
            StopScanTag();
            var dw = new DumpWindow(t, "", true);
            dw.ShowDialog();
            await PeriodicScanTag();
        }


        private void btnAbortCurrentTask_Click(object sender, RoutedEventArgs e)
        {
            ProcessCTS.Cancel();
            ShowAbortButton(false);
            ValidateActions(true);
        }
        private async void ckEnablePeriodicTagScan_Checked(object sender, RoutedEventArgs e)
        {


            if (ckEnablePeriodicTagScan.IsChecked.HasValue && ckEnablePeriodicTagScan.IsChecked.Value)
                await PeriodicScanTag();
        }

    }
}
