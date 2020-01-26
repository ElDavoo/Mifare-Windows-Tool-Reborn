using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace miLazyCracker_Windows
{
    public partial class frmMiLazyCracker : Form
    {
        AutoResetEvent doneEvent = new AutoResetEvent(false);
        private Process process = new Process();
        private bool lprocess = false;
        private bool running = false;
        string myUID = "";
        string TMPFILE_MFD = "";
        string TMPFILE_MFD2 = "";
        string TMPFILE_UNK = "";
        string TMPFILE_FND = "";
        action CurrentAction = action.Read;
        public frmMiLazyCracker()
        {
            InitializeComponent();
        }
        enum action { Read, Dump }
        private void btnStart_Click(object sender, EventArgs e)
        {

            rtbOutput.Text = "";
            if (CurrentAction == action.Read)
            {
                RunNfcList();

                if (!File.Exists(TMPFILE_MFD))
                {
                    RunMfoc();
                }
                if (File.Exists(TMPFILE_MFD) || rtbOutput.Text.Contains("Auth with all sectors succeeded, dumping keys to a file"))
                {
                    CurrentAction = action.Dump;
                    MessageBox.Show($"{(File.Exists(TMPFILE_MFD) ? "dump file already existing." : "")}put the blank card on the reader and press button Dump");
                }
                else
                {
                    rtbOutput.Text += "Auth : no success. Trying hardnested attack";
                }
            }

        }

        private void RunNfcMfcClassic()
        {
            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(mf_write);
            bgw.WorkerReportsProgress = true;
            bgw.ProgressChanged += new ProgressChangedEventHandler(default_rpt);
            bgw.RunWorkerAsync(new string[] { TMPFILE_MFD, TMPFILE_MFD2 });

        }

        void mf_write(object sender, DoWorkEventArgs e)
        {
            if (lprocess) { return; }
            ProcessStartInfo psi = new ProcessStartInfo("nfctools/nfc-mfclassic.exe");
            string[] args = (string[])e.Argument;
            psi.Arguments = $"W a \"{args[0]}\" \"{args[1]}\"";
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            lprocess = true;
            BackgroundWorker b = (BackgroundWorker)sender;
            process = Process.Start(psi);
            b.ReportProgress(0, "Running"); running = true;
            process.OutputDataReceived += (s, _e) => b.ReportProgress(0, _e.Data);
            process.ErrorDataReceived += (s, _e) => b.ReportProgress(0, _e.Data);
          
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            lprocess = false; running = false;
            b.ReportProgress(100, "Done");
        }

        private void RunMfoc(string keys = "keys/extended-std.keys")
        {
            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(mfoc);
            bgw.WorkerReportsProgress = true;
            bgw.ProgressChanged += new ProgressChangedEventHandler(default_rpt);

            bgw.RunWorkerAsync(new string[] { keys, TMPFILE_MFD, TMPFILE_UNK });
            doneEvent.WaitOne();
            Application.DoEvents();
            MessageBox.Show("done mfoc");

        }

        private void RunNfcList()
        {
            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(list_tag);
            bgw.WorkerReportsProgress = true;
            bgw.ProgressChanged += new ProgressChangedEventHandler(default_rpt);
            bgw.RunWorkerAsync();

            doneEvent.WaitOne();
            Application.DoEvents(); //This call is very important if you want to have a progress bar and want to update it
                                    //from the Progress event of the background worker.
            Thread.Sleep(1000);
        }

        private void logAppend(string msg)
        {
            rtbOutput.AppendText(msg + "\n");
            rtbOutput.ScrollToCaret();
        }
        private void default_rpt(object sender, ProgressChangedEventArgs e)
        {
            logAppend((string)e.UserState);

        }
        public delegate void InvokeDelegate();
        void list_tag(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (lprocess) { return; }
                ProcessStartInfo psi = new ProcessStartInfo(@"nfctools/nfc-list.exe");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                lprocess = true;
                BackgroundWorker b = (BackgroundWorker)sender;
                process = Process.Start(psi);
                b.ReportProgress(0, "running");
                running = true;
                process.OutputDataReceived += (s, _e) => b.ReportProgress(0, _e.Data);
                process.ErrorDataReceived += (s, _e) => b.ReportProgress(0, _e.Data);
                //StreamReader stderr = process.StandardError;
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                lprocess = false; running = false;
                b.ReportProgress(100, "done");
                rtbOutput.BeginInvoke(new InvokeDelegate(() =>
                {
                    var retUID = rtbOutput.Text.Split('\n').Where(t => t.Contains("UID")).FirstOrDefault();
                    if (retUID != null && retUID.Contains(": "))
                    {
                        myUID = retUID.Substring(retUID.IndexOf(": ") + ": ".Length).Replace(" ", "");
                        TMPFILE_UNK = $"mfc_{myUID}_unknownMfocSectorInfo.txt";
                        if (CurrentAction == action.Read)
                            TMPFILE_MFD = $"mfc_{myUID}_dump.mfd";
                        else
                            TMPFILE_MFD2 = $"mfc_{myUID}_dump.mfd";

                        TMPFILE_FND = $"mfc_{myUID}_foundKeys.txt";
                        this.Text = $"new UID Found : {myUID}";
                    }
                    else
                    {
                        this.Text = $"no tag found";
                    }
                }));
            }
            finally
            {
                doneEvent.Set();
            }

        }
        void mfoc(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (lprocess) { return; }
                ProcessStartInfo psi = new ProcessStartInfo("nfctools/mfoc.exe");
                string[] args = (string[])e.Argument;
                if (File.Exists(TMPFILE_FND))
                {
                    if (!string.IsNullOrWhiteSpace(args[0]))
                        psi.Arguments = $"-f \"{TMPFILE_FND}\" - f \"{args[0]}\" -O \"{args[1]}\" -D \"{args[2]}\"";
                    else
                        psi.Arguments = $"-f \"{TMPFILE_FND}\" -O \"{args[1]}\" -D \"{args[2]}\"";
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(args[0]))
                        psi.Arguments = $"-f \"{args[0]}\" -O \"{args[1]}\" -D \"{args[2]}\"";
                    else
                        psi.Arguments = $"-O \"{args[1]}\" -D \"{args[2]}\"";
                }

                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                lprocess = true;
                BackgroundWorker b = (BackgroundWorker)sender;
                process = Process.Start(psi);
                b.ReportProgress(0, "Starting MFOC decryption……"); running = true;
                process.OutputDataReceived += (s, _e) => b.ReportProgress(0, _e.Data);
                process.ErrorDataReceived += (s, _e) => b.ReportProgress(0, _e.Data);
                //StreamReader stderr = process.StandardError;
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                lprocess = false; running = false;
                if (process.ExitCode == 0)
                {
                    b.ReportProgress(101, "##finished##");
                }
                else
                {
                    b.ReportProgress(100, "done");
                    File.Delete(args[0]);
                }
            }
            finally
            {
                doneEvent.Set();
            }
        }

        private void btnDump_Click(object sender, EventArgs e)
        {
            if (CurrentAction == action.Dump)
            {
                RunNfcList();
                RunMfoc("");
                RunNfcMfcClassic();
            }
        }
    }
}
