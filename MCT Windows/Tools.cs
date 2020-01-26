using MCT_Windows.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MCT_Windows
{
    public class Tools
    {
        public AutoResetEvent doneEvent { get; set; } = new AutoResetEvent(false);
        public Process process = new Process();
        public bool lprocess = false;
        public bool running = false;
        MainWindow Main { get; set; }
        public Tools(MainWindow main)
        {
            Main = main;
        }

        public string mySourceUID { get; set; } = "";
        public string myTargetUID { get; set; } = "";
        public string TMPFILESOURCE_MFD { get; set; } = "";
        public string TMPFILE_TARGETMFD { get; set; } = "";
        public string TMPFILE_UNK { get; set; } = "";
        public string TMPFILE_FND { get; set; } = "";
        public void list_tag(object sender, DoWorkEventArgs e)
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
                process.EnableRaisingEvents = true;
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                lprocess = false;
                running = false;
                if (process.ExitCode == 0)
                {
                    b.ReportProgress(101, "##finished##");
                }
                else
                    b.ReportProgress(100, "done");
            }
            finally
            {
                doneEvent.Set();
            }

        }



        public void mfoc(object sender, DoWorkEventArgs e)
        {
            if (lprocess) { return; }

            ProcessStartInfo psi = new ProcessStartInfo("nfctools/mfoc.exe");
            string[] args = (string[])e.Argument;
            if (File.Exists(TMPFILE_FND))
            {
                if (args.Count() > 2)
                {
                    psi.Arguments += AddKeys(args);
                    psi.Arguments += $"-O \"{args[args.Count() - 2]}\" -D \"{args[args.Count() - 1]}\"";
                }
                else
                    psi.Arguments = $"-f \"{TMPFILE_FND}\" -O \"{args[1]}\" -D \"{args[2]}\"";
            }
            else
            {
                if (args.Count() > 2)
                {
                    psi.Arguments += AddKeys(args);
                    psi.Arguments += $" -O \"{args[args.Count() - 2]}\" -D \"{args[args.Count() - 1]}\"";
                }
                else
                    psi.Arguments = $"-O \"{args[1]}\" -D \"{args[2]}\"";
            }
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            process.EnableRaisingEvents = true;
            lprocess = true;
            BackgroundWorker b = (BackgroundWorker)sender;
            process = Process.Start(psi);
            b.ReportProgress(0, "Starting MFOC decryption……");
            running = true;
            process.OutputDataReceived += (s, _e) => b.ReportProgress(0, _e.Data);
            process.ErrorDataReceived += (s, _e) => b.ReportProgress(0, _e.Data);
            process.Exited += (s, _e) => Main.ShowDump();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                b.ReportProgress(101, "##mfoc finished##");

            }
            else
            {
                b.ReportProgress(100, "done with errors");
                File.Delete(args[0]);
            }
            lprocess = false;
            running = false;

        }

        private string AddKeys(string[] args)
        {
            var ret = "";
            for (int i = 0; i < args.Count() - 2; i++)
            {
                ret += $"  -f \"{args[i]}\"";
            }
            return ret;
        }

        public void mf_write(object sender, DoWorkEventArgs e)
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
            b.ReportProgress(0, "cloning...");
            running = true;
            process.OutputDataReceived += (s, _e) => b.ReportProgress(0, _e.Data);
            process.ErrorDataReceived += (s, _e) => b.ReportProgress(0, _e.Data);

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            if (process.ExitCode == 0)
            {
                b.ReportProgress(101, "##mfoc finished##");

            }
            else
            {
                b.ReportProgress(100, "done with errors");
                File.Delete(args[0]);
            }
            lprocess = false;
            running = false;

        }
    }
}
