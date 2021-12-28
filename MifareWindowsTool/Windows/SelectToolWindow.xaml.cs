using Gu.Wpf.Localization;

using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MCT_Windows.Windows
{
    /// <summary>
    /// Logique d'interaction pour SelectToolWindow.xaml
    /// </summary>
    public partial class SelectToolWindow : Window
    {
        Tools Tools { get; set; }
        MainWindow Main = null;

        public SelectToolWindow(MainWindow mainw, Tools t)
        {
            Tools = t;
            Main = mainw;
            InitializeComponent();
            Uri iconUri = new Uri("pack://application:,,,/Resources/MWT.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
            txtDumpsPath.Text = t.DefaultDumpPath;
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
    }
}
