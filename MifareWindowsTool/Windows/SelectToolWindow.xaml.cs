using Gu.Wpf.Localization;

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
        Tools tools = null;
        MainWindow Main = null;
        public SelectToolWindow(MainWindow mainw, Tools t)
        {
            tools = t;
            Main = mainw;
            InitializeComponent();
            Uri iconUri = new Uri("pack://application:,,,/Resources/MWT.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnCompareDumps_Click(object sender, RoutedEventArgs e)
        {
            Main.StopScanTag();
            var dw = new DumpWindow(tools, "", true);
            dw.ShowDialog();
            Main.PeriodicScanTag();
        }

        private void btnChangeUID_Click(object sender, RoutedEventArgs e)
        {
            Main.StopScanTag();
            var suw = new SetUIDWindow(Main, tools);
            suw.ShowDialog();
            Main.PeriodicScanTag();
        }

        private void btnLangFR_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnLangEN_Click(object sender, RoutedEventArgs e)
        {

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
    }
}
