using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Reflection;
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
        public bool nfcDeviceFound { get; set; } = true;
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


        public void ToggleACR122Enabled()
        {
            ToggleDeviceEnabled(new Guid("{50dd5230-ba8a-11d1-bf5d-0000f805f530}"), @"USB\VID_072F&PID_2200\7&AEC7C26&0&4");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceGuid"> every type of device has a hard-coded GUID</param>
        /// <param name="instancePath">get this from the properties dialog box of this device in Device Manager</param>
        private void ToggleDeviceEnabled(Guid deviceGuid, string instancePath)
        {
            DeviceHelper.ToggleDeviceEnabled(deviceGuid, instancePath);
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
        public bool UninstallLibUsbKDriver()
        {
            var oemAcrInfFilePath = @"C:\Windows\INF\oem71.inf";
            if (!File.Exists(oemAcrInfFilePath))
            {
                var message = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.GetInfoForManualDriverUninstall));
                var dr = MessageBox.Show(message, Translate.Key(nameof(MifareWindowsTool.Properties.Resources.Warning)), MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (dr == MessageBoxResult.Yes)
                {
                    Process.Start("https://github.com/xavave/Mifare-Windows-Tool/releases/tag/1.6.8408.42890");
                }
                return false;
            }
            var startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = "ACR122_LibUsbK_Driver";
            startInfo.FileName = "dpinst64.exe";
            startInfo.Arguments = $@"/U  {oemAcrInfFilePath} /S";
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
        internal (Version, Version) CheckNewVersion()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var urlReleases = "https://api.github.com/repos/xavave/Mifare-Windows-Tool/releases";
                    client.DefaultRequestHeaders.Add("User-Agent", "C# App");
                    // Create the HttpContent for the form to be posted.
                    var requestContent = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("username", "xavave") });

                    // Get the response.
                    HttpResponseMessage response = client.GetAsync(urlReleases).Result;

                    // Get the response content.
                    HttpContent responseContent = response.Content;
                    string data = "";
                    // Get the stream of the content.
                    using (var reader = new StreamReader(responseContent.ReadAsStreamAsync().Result))
                    {
                        // Write the output.
                        data = reader.ReadToEnd();
                    }
                    List<GitRelease> gitReleases = JsonConvert.DeserializeObject<List<GitRelease>>(data);
                    if (gitReleases == null || !gitReleases.Any()) return (null, null);
                    var latestVersionTagName = gitReleases.Where(r => r.prerelease == false).Select(s => s.tag_name).FirstOrDefault();
                    var latestVersion = new Version(latestVersionTagName);
                    var latestPreRelease = gitReleases.Where(r => r.prerelease == true).Select(s => s.tag_name).FirstOrDefault();
                    if (latestPreRelease == null) return (latestVersion, null);
                    var latestPreReleaseVersion = new Version(latestPreRelease);
                    return (latestVersion, latestPreReleaseVersion);
                }
                catch (Exception)
                {
                    return (null, null);
                }

            }
        }
        private void CheckNewVersions()
        {
            var newVersions = CheckNewVersion();
            var hasNewRelease = false;
            var hasNewPreRelease = false;
            var messageNewVersion = string.Empty;
            var urlLatest = "https://github.com/xavave/Mifare-Windows-Tool/releases/latest";
            var urlLatestBeta = string.Empty;
            var goOnGithubPage = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.GoOnGitHubPage));
            var goOnGithubPageBeta = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.GoOnGitHubPageBeta));
            if (newVersions.Item1 != null)
            {
                var comp = Assembly.GetExecutingAssembly().GetName().Version.CompareTo(newVersions.Item1);
                hasNewRelease = comp < 0;

            }
            if (newVersions.Item2 != null)
            {
                var comp = Assembly.GetExecutingAssembly().GetName().Version.CompareTo(newVersions.Item2);
                hasNewPreRelease = comp < 0;
                urlLatestBeta = $"https://github.com/xavave/Mifare-Windows-Tool/releases/tag/{newVersions.Item2}";

            }
            if (!hasNewRelease && !hasNewPreRelease) return;

            if (hasNewRelease)
            {
                messageNewVersion = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.NewerVersionExists));
            }
            if (hasNewPreRelease)
            {
                messageNewVersion = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.NewerBetaVersionExists));
            }

            if (hasNewRelease && !hasNewPreRelease)
            {
                messageNewVersion += goOnGithubPage;
                DisplayVersionPageChoice(messageNewVersion, urlLatest);
            }
            else if (!hasNewRelease && hasNewPreRelease)
            {
                messageNewVersion += goOnGithubPage;
                DisplayVersionPageChoice(messageNewVersion, urlLatestBeta);
            }
            else if (hasNewRelease && hasNewPreRelease)
            {
                messageNewVersion += goOnGithubPageBeta;
                var ret = MessageBox.Show(messageNewVersion, "Version", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                if (ret == MessageBoxResult.Yes)
                {
                    Process.Start(urlLatestBeta);
                }
                else if (ret == MessageBoxResult.No)
                {
                    Process.Start(urlLatest);
                }
            }

        }

        private static void DisplayVersionPageChoice(string messageNewVersion, string url)
        {
            var ret = MessageBox.Show(messageNewVersion, "Version", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (ret == MessageBoxResult.Yes)
            {
                Process.Start(url);
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
