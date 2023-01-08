using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Windows;

using MifareWindowsTool.Common;
using MifareWindowsTool.DumpClasses;
using MifareWindowsTool.Properties;

using Newtonsoft.Json;

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
    public enum DumpExistenceType
    {
        None,
        Source,
        Target,
        Invalid
    }
    public class Win32_PnPEntity
    {
        public string Caption { get; set; }
        public string Status { get; set; }
        public string HardwareID { get; set; }
    }
    public class Tools
    {
        public bool lprocess = false;
        public bool running = false;
        private List<string> NfcToolsMandatoryExeListToCheck = new List<string>()
           {
               "nfc-list.exe",
               "nfc-mfclassic.exe",
               "mfoc-hardnested.exe",
               "nfc-mfsetuid.exe"
           };
        public Tools()
        {

        }

        //public string TMPFILESOURCE_MFD { get; set; } = "";
        public IDump SourceBinaryDump { get; set; } = new DumpMWT();
        public IDump TargetBinaryDump { get; set; } = new DumpMWT();
        //public string TMPFILESOURCEPATH_MFD { get; set; } = "";
        //public string TMPFILE_TARGETMFD { get; set; } = "";
        public string TMPFILE_UNK { get; set; } = "";
        public string TMPFILE_FND { get; set; } = "";

        public string GetSetting(string key)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
                return ConfigurationManager.AppSettings[key];
            return string.Empty;
        }

        public void SetSetting(string key, string value)
        {
            try
            {
                Configuration configuration =
                       ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (configuration.AppSettings.Settings.AllKeys.Contains(key))
                    configuration.AppSettings.Settings[key].Value = value;
                else
                    configuration.AppSettings.Settings.Add(key, value);

                configuration.Save(ConfigurationSaveMode.Full, true);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"could not save settings : {ex}");
            }
        }

        public string ResetKeyPath()
        {
            DumpBase.DefaultKeysPath = System.IO.Path.Combine(DumpBase.DefaultWorkingDir, "keys");

            SetSetting("DefaultKeysPath", DumpBase.DefaultKeysPath);
            return DumpBase.DefaultKeysPath;
        }
        public string ChangeDefaultKeyPath()
        {
            using (var fd = new System.Windows.Forms.FolderBrowserDialog())
            {
                fd.SelectedPath = DumpBase.DefaultKeysPath;
                System.Windows.Forms.DialogResult result = fd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    DumpBase.DefaultKeysPath = fd.SelectedPath;

                    SetSetting("DefaultKeysPath", DumpBase.DefaultKeysPath);
                    return DumpBase.DefaultKeysPath;
                }
            }
            return null;
        }
        public bool CheckNfcToolsFolder(string fileToCheck = null)
        {
            var filesToCheck = new List<string>();
            if (!string.IsNullOrEmpty(fileToCheck))
            {
                filesToCheck.Add(fileToCheck);
            }
            else
            {
                filesToCheck = NfcToolsMandatoryExeListToCheck;
            }
            var existingFiles = Directory.EnumerateFiles(DumpBase.DefaultNfcToolsPath, "*.exe", SearchOption.TopDirectoryOnly).Select(p => Path.GetFileName(p)).ToList();
            var comparelst = filesToCheck.Except(existingFiles).ToList();
            if (comparelst.Any())
            {
                var message = "";
                if (string.IsNullOrEmpty(fileToCheck))
                {
                    message = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.MissingMandatoryExeFiles)) +
                      DumpBase.DefaultNfcToolsPath + ":\n"
                      + string.Join("\n", comparelst);
                }
                else
                {
                    message = string.Format(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.MissingExeFile)), DumpBase.DefaultNfcToolsPath, fileToCheck);
                }
                MessageBox.Show(message, Translate.Key(nameof(MifareWindowsTool.Properties.Resources.Warning)), MessageBoxButton.OK, MessageBoxImage.Warning);

                return false;
            }
            return true;
        }
        public bool InstallLibUsbKDriver()
        {
            var startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = "ACR122_LibUsbK_Driver";
            startInfo.FileName = "InstallDriver.exe";
            Process proc = Process.Start(startInfo);
            proc.WaitForExit();
            return proc.ExitCode == 0;
        }
        public List<string> GetDrivers()
        {
            var col = new List<string>();
            //Declare, Search, and Get the Properties in Win32_SystemDriver
            System.Management.SelectQuery query = new System.Management.SelectQuery("Win32_SystemDriver");
            System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher(query);
            foreach (System.Management.ManagementObject ManageObject in searcher.Get())
            {
                //Declare the Main Item
                var name = ManageObject["Name"].ToString() + " - " + ManageObject["State"].ToString(); // + " - " + ManageObject["Description"].ToString();
                col.Add(name);

            }
            return col;
        }
        //public void test()
        //{
        //    WMIHelper helper = new WMIHelper("root\\CimV2");
        //    var acr = helper.Query("SELECT hardwareID FROM Win32_PnPEntity where name like '%ACR122%'");

        //}
        public string DriverState(string driverName = "ACR122U")
        {
            System.Management.SelectQuery query = new System.Management.SelectQuery("Win32_SystemDriver");
            query.Condition = $"Name like '%{driverName}%'";
            if (running)
                query.Condition += " AND State = 'running'";
            System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher(query);
            var drivers = searcher.Get();

            if (drivers.Count > 0)
            {
                var dr = drivers.OfType<ManagementObject>().First();
                return dr["State"].ToString().ToLower();
            }


            return "";
        }
        internal Version CheckNewVersion()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "C# App");
                    // Create the HttpContent for the form to be posted.
                    var requestContent = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("username", "xavave") });

                    // Get the response.
                    HttpResponseMessage response = client.GetAsync("https://api.github.com/repos/xavave/Mifare-Windows-Tool/releases/latest").Result;

                    // Get the response content.
                    HttpContent responseContent = response.Content;
                    string data = "";
                    // Get the stream of the content.
                    using (var reader = new StreamReader(responseContent.ReadAsStreamAsync().Result))
                    {
                        // Write the output.
                        data = reader.ReadToEnd();
                    }
                    var definition = new { tag_name = "" };
                    var latestVersionTagName = JsonConvert.DeserializeAnonymousType(data, definition);
                    var latestVersion = new Version(latestVersionTagName.tag_name);
                    return latestVersion;
                }
                catch (Exception)
                {
                    return null;
                }

            }
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

        public bool CheckAndUseDumpIfExists(IDump dump, TagAction act, out bool forceReRead, bool silentMode = false)
        {
            forceReRead = false;
            if (System.IO.File.Exists(dump.DumpFileFullName))
            {
                long fileLength = new System.IO.FileInfo(dump.DumpFileFullName).Length;
                if (fileLength == 0) return false;
                if (!silentMode)
                {
                    var msg = act == TagAction.ReadTarget ? Translate.Key(nameof(MifareWindowsTool.Properties.Resources.ReadTargetMoreInfos)) + " " : "";
                    msg += Translate.Key(nameof(MifareWindowsTool.Properties.Resources.BadgeUIDAlreadyknown));
                    var dr = MessageBox.Show(msg, Translate.Key(nameof(MifareWindowsTool.Properties.Resources.DumpExisting)) + $" ({dump.DumpFileName})", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    forceReRead = dr == MessageBoxResult.Yes;
                    return (dr == MessageBoxResult.No);
                }
                else
                    return true;
            }
            return false;
        }

        internal string ChangeDefaultDumpPath()
        {
            using (var fd = new System.Windows.Forms.FolderBrowserDialog())
            {
                fd.SelectedPath = DumpBase.DefaultDumpPath;
                System.Windows.Forms.DialogResult result = fd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    DumpBase.DefaultDumpPath = fd.SelectedPath;
                    SetSetting("DefaultDumpPath", DumpBase.DefaultDumpPath);
                    return DumpBase.DefaultDumpPath;
                }
                return null;
            }
        }

        internal string ResetDumpPath()
        {
            DumpBase.DefaultDumpPath = System.IO.Path.Combine(DumpBase.DefaultWorkingDir, "dumps");
            SetSetting("DefaultDumpPath", DumpBase.DefaultDumpPath);
            return DumpBase.DefaultDumpPath;
        }
    }
}
