using MCT_Windows.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
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
        public bool ScanTagRunning = false;
        string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public string MainTitle { get; set; } = $"Mifare Windows Tool";
        Tools t = null;
        OpenFileDialog ofd = new OpenFileDialog();
        public List<File> SelectedKeys = new List<File>();
        Uri BaseUri = null;
        IObservable<long> ObservableScan = null;
        CancellationTokenSource ScanSource = null;
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
            PeriodicScanTag();

        }

        public void PeriodicScanTag(int delay = 0)
        {
            if (ScanTagRunning) return;

            Task.Delay(delay);

            ObservableScan = Observable.Interval(TimeSpan.FromSeconds(3));
            // Token for cancelation
            ScanSource = new CancellationTokenSource();
            // Subscribe the obserable to the task on execution.
            ObservableScan.Subscribe(x =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (!ckEnablePeriodicTagScan.IsChecked.HasValue || ckEnablePeriodicTagScan.IsChecked.Value == false) return;
                    rtbOutput.Text = $"{DateTime.Now} -  {MifareWindowsTool.Properties.Resources.AutoScanTagRunning}...\n";
                    RunNfcList();
                }));
            }, ScanSource.Token);
            ScanTagRunning = true;
        }

        private void btnReadTag_Click(object sender, RoutedEventArgs e)
        {
            ReadTag(TagAction.ReadSource);

        }
        private void btnWriteTag_Click(object sender, RoutedEventArgs e)
        {
            ReadTag(TagAction.ReadTarget);
        }

        private void ReadTag(TagAction act)
        {

            if (t.running) return;

            if (Title.Contains("no tag") && !ScanTagRunning)
                PeriodicScanTag();

            if (!ckEnablePeriodicTagScan.IsChecked.HasValue || !ckEnablePeriodicTagScan.IsChecked.Value)
            {
                RunNfcList();
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
                        TagFound = true;
                        return;
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
                        TagFound = true;
                    }
                }
            }

            if (TagFound)
            {
                StopScanTag();
                if (act == TagAction.ReadSource)
                {
                    MapKeyToSectorWindow mtsWin = new MapKeyToSectorWindow(this, t, MifareWindowsTool.Properties.Resources.UsedForSourceMapping);
                    var ret = mtsWin.ShowDialog();
                    if (ret.HasValue && ret.Value)
                        RunMfoc(SelectedKeys, t.TMPFILESOURCE_MFD);
                    else
                        PeriodicScanTag();
                }
                else if (act == TagAction.ReadTarget)
                {
                    WriteDumpWindow wdw = new WriteDumpWindow(this, t);
                    var ret = wdw.ShowDialog();
                    if (!ret.HasValue || !ret.Value)
                        PeriodicScanTag();

                }

                TagFound = !Title.Contains(MifareWindowsTool.Properties.Resources.NoTag);
            }
            else
            {
                logAppend(MifareWindowsTool.Properties.Resources.NoTagDetectedOnReader);
            }

        }



        public void ValidateActions()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)delegate
            {
                btnReadTag.IsEnabled = !t.running;
                btnWriteTag.IsEnabled = !t.running;
                ckEnablePeriodicTagScan.IsEnabled = !t.running;
            });
        }

        public void ShowAbortButton()
        {
            if (!t.process.HasExited)
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    btnAbortCurrentTask.Content = $"{MifareWindowsTool.Properties.Resources.Abort} {t.process.ProcessName}";
                    btnAbortCurrentTask.Visibility = Visibility.Visible;
                });
            }
        }
        public void HideAbortButton()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)delegate
            {
                btnAbortCurrentTask.Visibility = Visibility.Hidden;
            });
        }

        public void ShowDump()
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                DumpWindow dw = new DumpWindow(t, t.TMPFILESOURCE_MFD);
                var dr = dw.ShowDialog();
                if (dr.HasValue && dr.Value)
                    PeriodicScanTag();
            });

        }

        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                  new Action(delegate { }));
        }

        private void RunNfcList()
        {
            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(t.list_tag);
            bgw.WorkerReportsProgress = true;
            bgw.ProgressChanged += new ProgressChangedEventHandler(default_rpt);

            bgw.RunWorkerAsync();
            t.doneEvent.WaitOne();
            DoEvents();

            var retUID = rtbOutput.Text.Split('\n').Where(t => t.Contains("UID")).LastOrDefault();
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
                ScanSource.Cancel();
                logAppend(MifareWindowsTool.Properties.Resources.AutoScanTagStopped);
            }
        }
        public void RunMifareClassicFormat()
        {
            StopScanTag();

            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(t.classic_format);
            bgw.WorkerReportsProgress = true;
            bgw.ProgressChanged += new ProgressChangedEventHandler(default_rpt);
            bgw.RunWorkerAsync(new string[] { "dumps\\" + t.TMPFILE_TARGETMFD });

        }
        public void RunNfcMfcClassic(TagAction act, bool bWriteBlock0, bool useKeyA, bool haltOnError)
        {
            StopScanTag();

            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(t.mf_write);
            bgw.WorkerReportsProgress = true;
            bgw.ProgressChanged += new ProgressChangedEventHandler(default_rpt);
            bgw.RunWorkerAsync(new string[] { act.ToString(), "dumps\\" + t.TMPFILESOURCE_MFD, "dumps\\" + t.TMPFILE_TARGETMFD, bWriteBlock0.ToString(), useKeyA.ToString(), haltOnError.ToString() });

        }
        public void RunMfoc(List<File> keys, string tmpFileMfd)
        {
            try
            {
                StopScanTag();
                BackgroundWorker bgw = new BackgroundWorker();
                bgw.DoWork += new DoWorkEventHandler(t.mfoc);
                bgw.WorkerReportsProgress = true;
                bgw.ProgressChanged += new ProgressChangedEventHandler(default_rpt);
                var parameters = keys.Select(k => "keys/" + k.FileName).ToList();
                parameters.Add("dumps\\" + tmpFileMfd);
                parameters.Add(t.TMPFILE_UNK);
                bgw.RunWorkerAsync(parameters.ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                PeriodicScanTag();
            }


        }

        public void logAppend(string msg)
        {
            rtbOutput.AppendText(msg + "\n");

        }
        private void default_rpt(object sender, ProgressChangedEventArgs e)
        {
            logAppend((string)e.UserState);

        }

        private void btnEditAddKeyFile_Click(object sender, RoutedEventArgs e)
        {
            StopScanTag();
            SelectKeyFilesWindow ekf = new SelectKeyFilesWindow(this, t);
            ekf.ShowDialog();
            PeriodicScanTag();
        }

        private void rtbOutput_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }


        private void btnOpenExistingTag_Click(object sender, RoutedEventArgs e)
        {
            OpenTag(TagAction.ReadSource);
        }

        private void OpenTag(TagAction act)
        {
            var dr = ofd.ShowDialog();
            if (dr.Value)
            {
                if (act == TagAction.ReadSource)
                    t.TMPFILESOURCE_MFD = ofd.FileName;
                else if (act == TagAction.ReadTarget)
                    t.TMPFILE_TARGETMFD = ofd.FileName;
            }
        }

        private void btnEditDumpFile_Click(object sender, RoutedEventArgs e)
        {
            StopScanTag();
            var dr = ofd.ShowDialog();
            if (dr.Value)
            {

                t.TMPFILESOURCE_MFD = ofd.FileName;
                ShowDump();
            }
            else
                PeriodicScanTag();

        }

        private void btnInfos_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/xavave/Mifare-Windows-Tool");
        }

        private void btnTools_Click(object sender, RoutedEventArgs e)
        {
            StopScanTag();
            var dw = new DumpWindow(t, "", true);
            dw.ShowDialog();
            PeriodicScanTag();
        }

        private void ckEnablePeriodicTagScan_Checked(object sender, RoutedEventArgs e)
        {
            if (ckEnablePeriodicTagScan.IsChecked.HasValue && ckEnablePeriodicTagScan.IsChecked.Value)
                PeriodicScanTag();
        }

        private void btnAbortCurrentTask_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (t.process != null && !t.process.HasExited)
                {
                    string processName = t.process.ProcessName;
                    t.process.Kill();
                    t.process.Dispose();
                    t.running = false;
                    rtbOutput.AppendText($"{processName} {MifareWindowsTool.Properties.Resources.Aborted}!\n");
                    HideAbortButton();
                    ValidateActions();
                }
            }
            catch (InvalidOperationException)
            {

            }
        }
    }
}
