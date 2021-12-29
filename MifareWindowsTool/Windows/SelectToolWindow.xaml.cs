using Gu.Wpf.Localization;

using MifareWindowsTool.Common;

using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace MCT_Windows.Windows
{
    /// <summary>
    /// Logique d'interaction pour SelectToolWindow.xaml
    /// </summary>
    public partial class SelectToolWindow : Window
    {
        Tools Tools { get; set; }
        MainWindow Main = null;
        OpenFileDialog ofd1 = new OpenFileDialog();
        SaveFileDialog sfd1 = new SaveFileDialog();

        public SelectToolWindow(MainWindow mainw, Tools t)
        {
            Tools = t;
            Main = mainw;
            InitializeComponent();
            Uri iconUri = new Uri("pack://application:,,,/Resources/MWT.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
            txtDumpsPath.Text = t.DefaultDumpPath;
            txtKeysPath.Text = t.DefaultKeysPath;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnCompareDumps_Click(object sender, RoutedEventArgs e)
        {
            Main.StopScanTag();
            var dw = new DumpWindow(Tools, "", true);
            dw.ShowDialog();
            Main.PeriodicScanTag();
        }

        private void btnChangeUID_Click(object sender, RoutedEventArgs e)
        {
            Main.StopScanTag();
            var suw = new SetUIDWindow(Main, Tools);
            suw.ShowDialog();
            Main.PeriodicScanTag();
        }
        private void LangSelector_Loaded(object sender, RoutedEventArgs e)
        {
            var ls = sender as LanguageSelector;
            ls.Languages.Add(new Gu.Wpf.Localization.Language(new System.Globalization.CultureInfo("en-US")) { FlagSource = new Uri("pack://application:,,,/Gu.Wpf.Localization;component/Flags/en.png") });
            ls.Languages.Add(new Gu.Wpf.Localization.Language(new System.Globalization.CultureInfo("fr-FR")) { FlagSource = new Uri("pack://application:,,,/Gu.Wpf.Localization;component/Flags/fr.png") });

        }

        private void btnReinstallLibUsbK_Click(object sender, RoutedEventArgs e)
        {
            new Tools(Main).InstallLibUsbKDriver();
        }

        private void btnChangeDefaultDumpPath_Click(object sender, RoutedEventArgs e)
        {
            using (var fd = new System.Windows.Forms.FolderBrowserDialog())
            {
                fd.SelectedPath = Tools.DefaultDumpPath;
                System.Windows.Forms.DialogResult result = fd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    Tools.DefaultDumpPath = fd.SelectedPath;
                    txtDumpsPath.Text = Tools.DefaultDumpPath;
                    Tools.SetSetting(Tools.ConstDefaultDumpPath, Tools.DefaultDumpPath);
                }
            }

        }

        private void btnResetDumpPath_Click(object sender, RoutedEventArgs e)
        {
            Tools.DefaultDumpPath = System.IO.Path.Combine(Tools.DefaultWorkingDir, "dumps");
            txtDumpsPath.Text = Tools.DefaultDumpPath;
            Tools.SetSetting(Tools.ConstDefaultDumpPath, Tools.DefaultDumpPath);
        }


        private void btnChangeDefaultKeyPath_Click(object sender, RoutedEventArgs e)
        {
            txtKeysPath.Text = Tools.ChangeDefaultKeyPath();
        }


        private void btnResetKeyPath_Click(object sender, RoutedEventArgs e)
        {
            txtKeysPath.Text = Tools.ResetKeyPath();
        }

        private void btnConvertDump_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ret = ofd1.ShowDialog();
                if (ret == System.Windows.Forms.DialogResult.OK)
                {
                    Dump dump = new Dump();
                    DumpConverter converter = new DumpConverter();
                    var inputFileType = converter.CheckDump(ofd1.FileName);
                    if (inputFileType == FileType.Text)
                    {
                        //lblInfos.Text = "text dump detected";
                        dump = converter.ConvertToBinaryDump(dump);
                        sfd1.FileName = Path.GetFileNameWithoutExtension(ofd1.FileName) + ".mfd";
                        sfd1.FilterIndex = 1;
                    }
                    else
                    {
                        //lblInfos.Text = "binary dump detected";
                        dump = converter.ConvertToTextDump(ofd1.FileName);
                        sfd1.FileName = Path.GetFileNameWithoutExtension(ofd1.FileName) + ".txt";
                        sfd1.FilterIndex = 2;
                    }

                    sfd1.InitialDirectory = ofd1.InitialDirectory;
                    ret = sfd1.ShowDialog();
                    if (ret == System.Windows.Forms.DialogResult.OK)
                    {

                        if (inputFileType == FileType.Text)
                        {
                            File.WriteAllBytes(sfd1.FileName, dump.BinaryOutput.ToArray());
                        }
                        else
                        {
                            dump = converter.ConvertToTextDump(ofd1.FileName);
                            File.WriteAllText(sfd1.FileName, dump.TextOutput);

                        }

                        System.Windows.MessageBox.Show($"Conversion to {(inputFileType == FileType.Binary ? "Text dump" : "Binary dump")}: Done");
                        //lblInfos.Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);

            }
        }
    }
}
