using CliWrap;
using MCT_Windows.Windows;
using MifareWindowsTool.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;

namespace MCT_Windows
{
    public enum TagAction
    {
        None,
        ReadSource,
        ReadTarget,
        Clone,
        Format_PassA,
        Format_PassB,
    }
    public enum TagType
    {
        Not0Writable,
        UnlockedGen1,
        DirectCUIDgen2
    }
    public enum DumpExists
    {
        None,
        Source,
        Target,
        Both
    }

    public class Tools
    {
        public string DefaultWorkingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        MediaPlayer Player = null;
        public bool lprocess = false;
        public bool running = false;
        public string CurrentUID = "";
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

        public Task<int> RunProcessAsync(string fileName, string arguments)
        {
            var tcs = new TaskCompletionSource<int>();

            var process = new Process
            {
                StartInfo = {
                    FileName = fileName,
                    Arguments = arguments,
                CreateNoWindow=true,
                UseShellExecute = false,
            WorkingDirectory = DefaultWorkingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,

        },
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();

            return tcs.Task;
        }
        internal bool TestWritePermission(string dirPath, bool throwIfFails = false)
        {
            try
            {
                using (FileStream fs = System.IO.File.Create(
                    Path.Combine(
                        dirPath,
                        Path.GetRandomFileName()
                    ),
                    1,
                    FileOptions.DeleteOnClose)
                )
                { }
                return true;
            }
            catch
            {
                if (throwIfFails)
                    throw;
                else
                    return false;
            }
        }

        public void PlayBeep(Uri baseUri)
        {
            try
            {
                Player = new MediaPlayer();
                Player.Open(new Uri("Beep_ACR122U.m4a", UriKind.RelativeOrAbsolute));
                Player.Play();

            }
            catch (Exception)
            {
            }
        }

        public bool CheckAndUseDumpIfExists(string MFDFile)
        {
            if (System.IO.File.Exists("dumps\\" + MFDFile))
            {
                long fileLength = new System.IO.FileInfo("dumps\\" + MFDFile).Length;
                if (fileLength == 0) return false;
                var dr = MessageBox.Show($"{Translate.Key(nameof(MifareWindowsTool.Properties.Resources.ADumpFile))} ({Path.GetFileName("dumps\\" + MFDFile)}) {Translate.Key(nameof(MifareWindowsTool.Properties.Resources.AlreadyExists))}, {Translate.Key(nameof(MifareWindowsTool.Properties.Resources.DoYouWantToReUseThisDump))}",
                    Translate.Key(nameof(MifareWindowsTool.Properties.Resources.DumpExisting)), MessageBoxButton.YesNo, MessageBoxImage.Question);

                return (dr == MessageBoxResult.Yes);
            }
            return false;
        }

       
    }
}
