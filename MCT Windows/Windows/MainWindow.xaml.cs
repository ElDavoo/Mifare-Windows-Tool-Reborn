using MCT_Windows.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        public string MainTitle { get; set; } = "MCT Windows";
        Tools t = null;
        enum action { ReadSource, ReadTarget, Dump }
        public List<Keys> SelectedKeys = new List<Keys>();
        action CurrentAction = action.ReadSource;
        public MainWindow()
        {
            InitializeComponent();
            t = new Tools(this);
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
            rtbOutput.Text = "";
            RunNfcList();

            var retUID = rtbOutput.Text.Split('\n').Where(t => t.Contains("UID")).FirstOrDefault();
            if (retUID != null && retUID.Contains(": "))
            {
                if (act == action.ReadSource)
                {
                    t.mySourceUID = retUID.Substring(retUID.IndexOf(": ") + ": ".Length).Replace(" ", "");
                    t.TMPFILE_UNK = $"mfc_{ t.mySourceUID}_unknownMfocSectorInfo.txt";
                    t.TMPFILESOURCE_MFD = $"mfc_{ t.mySourceUID}.dump";
                    t.TMPFILE_FND = $"mfc_{ t.mySourceUID}_foundKeys.txt";
                }
                else if (act == action.ReadTarget)
                {
                    t.myTargetUID = retUID.Substring(retUID.IndexOf(": ") + ": ".Length).Replace(" ", "");
                    t.TMPFILE_UNK = $"mfc_{ t.myTargetUID}_unknownMfocSectorInfo.txt";
                    t.TMPFILE_TARGETMFD = $"mfc_{ t.myTargetUID}.dump";
                    t.TMPFILE_FND = $"mfc_{ t.myTargetUID}_foundKeys.txt";
                }
                this.Title = $"{MainTitle} - new UID Found : { t.mySourceUID}";
                TagFound = true;
            }
            else
            {
                TagFound = false;
                this.Title = $"{MainTitle}: no tag found";
            }

            if (TagFound)
            {
                MapKeyToSectorWindow mtsWin = new MapKeyToSectorWindow(this, t);
                mtsWin.ShowDialog();
                if (act == action.ReadSource)
                    RunMfoc(SelectedKeys, t.TMPFILESOURCE_MFD);
                else if (act == action.ReadTarget)
                    RunMfoc(SelectedKeys, t.TMPFILE_TARGETMFD);

                TagFound = false;
            }
        }

        public void ShowDump()
        {

            Application.Current.Dispatcher.Invoke((Action)delegate
        {
            DumpWindow dw = new DumpWindow(t.TMPFILESOURCE_MFD);
            dw.Show();


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
        }

        private void RunNfcMfcClassic()
        {
            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(t.mf_write);
            bgw.WorkerReportsProgress = true;
            bgw.ProgressChanged += new ProgressChangedEventHandler(default_rpt);
            bgw.RunWorkerAsync(new string[] { t.TMPFILESOURCE_MFD, t.TMPFILE_TARGETMFD });

        }
        private void RunMfoc(List<Keys> keys, string tmpFileMfd)
        {
            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(t.mfoc);
            bgw.WorkerReportsProgress = true;
            bgw.ProgressChanged += new ProgressChangedEventHandler(default_rpt);
            var parameters = keys.Select(k => "keys/" + k.FileName).ToList();
            parameters.Add(tmpFileMfd);
            parameters.Add(t.TMPFILE_UNK);
            bgw.RunWorkerAsync(parameters.ToArray());


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

        }

        private void rtbOutput_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }

        private void btnWriteTag_Click(object sender, RoutedEventArgs e)
        {

        }



        private void btnOpenExistingTag_Click(object sender, RoutedEventArgs e)
        {
            OpenTag(action.ReadSource);
        }

        private void OpenTag(action act)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Dump Files|*.dump|All Files|*.*";
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

        private void btnOpenExistingTargetTag_Click(object sender, RoutedEventArgs e)
        {
            OpenTag(action.ReadTarget);
        }

        private void btnCloneTag_Click(object sender, RoutedEventArgs e)
        {
            RunNfcMfcClassic();
        }
    }
}
