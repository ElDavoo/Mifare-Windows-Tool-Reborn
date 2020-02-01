using System;
using System.Collections.Generic;
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

        private async void btnCompareDumps_Click(object sender, RoutedEventArgs e)
        {
            Main.StopScanTag();
            var dw = new DumpWindow(tools, "", true);
            dw.ShowDialog();
            await Main.PeriodicScanTag();
        }

        private async void btnChangeUID_Click(object sender, RoutedEventArgs e)
        {
            Main.StopScanTag();
            var suw = new SetUIDWindow(Main, tools);
            suw.ShowDialog();
            await Main.PeriodicScanTag();
        }
    }
}
