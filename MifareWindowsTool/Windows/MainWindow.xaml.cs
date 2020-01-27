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
        enum action { ReadSource, ReadTarget, Dump }
        public List<Keys> SelectedKeys = new List<Keys>();

        IObservable<long> ObservableScan = null;
        CancellationTokenSource ScanSource = null;
        public MainWindow()
        {
            InitializeComponent();
            MainTitle += $" v{version}";
            this.Title = $"{MainTitle}";
            t = new Tools(this);

            PeriodicScanTag();

        }

        public void PeriodicScanTag()
        {
            if (ScanTagRunning) return;
            ObservableScan = Observable.Interval(TimeSpan.FromSeconds(3));
            // Token for cancelation
            ScanSource = new CancellationTokenSource();
            // Subscribe the obserable to the task on execution.
            ObservableScan.Subscribe(x =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    rtbOutput.Text = "";
                    RunNfcList();
                }));
            }, ScanSource.Token);
            ScanTagRunning = true;
        }

        private void btnReadTag_Click(object sender, RoutedEventArgs e)
        {
            ReadTag(action.ReadSource);

        }
        private void btnReadTargetTag_Click(object sender, RoutedEventArgs e)
        {
            ReadTag(action.ReadTarget);
        }

        private void ReadTag(action act)
        {
            if (!string.IsNullOrWhiteSpace(t.CurrentUID))
            {
                if (act == action.ReadSource)
                {
                    t.mySourceUID = t.CurrentUID;
                    t.TMPFILE_UNK = $"mfc_{ t.mySourceUID}_unknownMfocSectorInfo.txt";
                    t.TMPFILESOURCE_MFD = $"mfc_{ t.mySourceUID}.dump";
                    t.TMPFILE_FND = $"mfc_{ t.mySourceUID}_foundKeys.txt";
                }
                else if (act == action.ReadTarget)
                {
                    t.myTargetUID = t.CurrentUID;
                    t.TMPFILE_UNK = $"mfc_{ t.myTargetUID}_unknownMfocSectorInfo.txt";
                    t.TMPFILE_TARGETMFD = $"mfc_{ t.myTargetUID}.dump";
                    t.TMPFILE_FND = $"mfc_{ t.myTargetUID}_foundKeys.txt";
                }
            }

            if (TagFound)
            {

                if (act == action.ReadSource)
                {
                    MapKeyToSectorWindow mtsWin = new MapKeyToSectorWindow(this, t);
                    mtsWin.ShowDialog();
                    RunMfoc(SelectedKeys, t.TMPFILESOURCE_MFD);
                }
                else if (act == action.ReadTarget)
                {
                    WriteDumpWindow wdw = new WriteDumpWindow(this, t);
                    wdw.ShowDialog();

                }

                TagFound = false;
            }
            else
            {
                MessageBox.Show("No Tag to read");
            }
        }

        public void ShowDump()
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {

                DumpWindow dw = new DumpWindow(t, t.TMPFILESOURCE_MFD);
                dw.ShowDialog();
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
            var retUID = rtbOutput.Text.Split('\n').Where(t => t.Contains("UID")).FirstOrDefault();
            if (retUID != null && retUID.Contains(": "))
            {
                t.CurrentUID = retUID.Substring(retUID.IndexOf(": ") + ": ".Length).Replace(" ", "").ToUpper();
                this.Title = $"{MainTitle}: new UID Found : { t.CurrentUID}";
                if (!TagFound)
                {
                    SystemSounds.Beep.Play();
                }
                TagFound = true;

            }

            else
            {
                t.CurrentUID = "";
                TagFound = false;
                this.Title = $"{MainTitle}: no tag";
            }
        }
        public void StopScanTag()
        {
            ScanTagRunning = false;
            ScanSource.Cancel();
        }

        public void RunNfcMfcClassic(bool bWriteBlock0)
        {
            StopScanTag();

            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(t.mf_write);
            bgw.WorkerReportsProgress = true;
            bgw.ProgressChanged += new ProgressChangedEventHandler(default_rpt);
            bgw.RunWorkerAsync(new string[] { t.TMPFILESOURCE_MFD, t.TMPFILE_TARGETMFD, bWriteBlock0.ToString() });

        }
        public void RunMfoc(List<Keys> keys, string tmpFileMfd)
        {
            try
            {
                StopScanTag();
                BackgroundWorker bgw = new BackgroundWorker();
                bgw.DoWork += new DoWorkEventHandler(t.mfoc);
                bgw.WorkerReportsProgress = true;
                bgw.ProgressChanged += new ProgressChangedEventHandler(default_rpt);
                var parameters = keys.Select(k => "keys/" + k.FileName).ToList();
                parameters.Add(tmpFileMfd);
                parameters.Add(t.TMPFILE_UNK);
                bgw.RunWorkerAsync(parameters.ToArray());
            }
            catch (Exception)
            {
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
            OpenTag(action.ReadSource);
        }

        private void OpenTag(action act)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Dump Files|*.dump;*.mfd|All Files|*.*";
            ofd.InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dr = ofd.ShowDialog();
            if (dr.Value)
            {
                if (act == action.ReadSource)
                    t.TMPFILESOURCE_MFD = ofd.FileName;
                else if (act == action.ReadTarget)
                    t.TMPFILE_TARGETMFD = ofd.FileName;
            }
        }

        private void btnEditDumpFile_Click(object sender, RoutedEventArgs e)
        {
            StopScanTag();
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Dump Files|*.dump;*.mfd|All Files|*.*";
            ofd.InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dr = ofd.ShowDialog();
            if (dr.Value)
            {
                t.TMPFILESOURCE_MFD = ofd.FileName;
                ShowDump();
            }
        }

        private void btnInfos_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/xavave/MifareDumper");
        }

        private void btnTools_Click(object sender, RoutedEventArgs e)
        {
            StopScanTag();
            var dw = new DumpWindow(t, "", true);
            dw.ShowDialog();
            PeriodicScanTag();
        }
    }
}
