using System;
using System.Windows;
using System.Windows.Media.Imaging;

using Gu.Wpf.Localization;

using MifareWindowsTool.Common;

using Path = System.IO.Path;

namespace MCT_Windows.Windows
{
    /// <summary>
    /// Logique d'interaction pour SelectToolWindow.xaml
    /// </summary>
    public partial class SelectToolWindow : Window
    {
        Tools Tools { get; }
        MainWindow Main { get; }

        public SelectToolWindow(MainWindow mainw, Tools t)
        {
            Tools = t;
            Main = mainw;
            InitializeComponent();
            Uri iconUri = new Uri("pack://application:,,,/Resources/MWT.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
            txtDumpsPath.Text = DumpBase.DefaultDumpPath; //TODO
            txtKeysPath.Text = DumpBase.DefaultKeysPath;

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnCompareDumps_Click(object sender, RoutedEventArgs e)
        {
            Main.StopScanTag();
            var dw = new DumpWindow();
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

        private void btnChangeDefaultDumpPath_Click(object sender, RoutedEventArgs e)
        {
            var tmpPath = Tools.ChangeDefaultDumpPath();
            if (tmpPath == null) return;
            txtDumpsPath.Text = tmpPath;

        }

        private void btnResetDumpPath_Click(object sender, RoutedEventArgs e)
        {
            txtDumpsPath.Text = Tools.ResetDumpPath();
        }


        private void btnChangeDefaultKeyPath_Click(object sender, RoutedEventArgs e)
        {
            var tmpPath = Tools.ChangeDefaultKeyPath();
            if (tmpPath == null) return;
            txtKeysPath.Text = tmpPath;
        }


        private void btnResetKeyPath_Click(object sender, RoutedEventArgs e)
        {

            txtKeysPath.Text = Tools.ResetKeyPath();
        }

        private void btnConvertDump_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                IDump dmp = DumpBase.OpenCreateDump(out bool canceled, "Source Dump");
                if (dmp == null) return;

                var selectOutputDumpWindow = new SelectOutputDumpTypeWindow(dmp);
                selectOutputDumpWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);

            }
        }
    }
}
