using Microsoft.Win32;

using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace MCT_Windows
{
    /// <summary>
    /// Logique d'interaction pour MapKeyToSectorWindow.xaml
    /// </summary>
    public partial class WriteDumpWindow : Window
    {
        Tools tools;
        MainWindow main;
        public WriteDumpWindow(MainWindow mainw, Tools t)
        {
            tools = t;
            main = mainw;
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void default_rpt(object sender, ProgressChangedEventArgs e)
        {
            main.logAppend((string)e.UserState);

        }

        private void btnWriteDump_Click(object sender, RoutedEventArgs e)
        {
            if (main.SelectedKeys.Any())
                main.RunNfcMfcClassic(ckEnableBlock0Writing.IsChecked.HasValue && ckEnableBlock0Writing.IsChecked.Value == true);
            else
                MessageBox.Show("You need to select at least one key file");
        }

        private void btnSelectDump_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Dump Files|*.dump;*.mfd";
            var dr = ofd.ShowDialog();
            if (dr.Value)
            {
                tools.TMPFILESOURCE_MFD = $"mfc_{ tools.mySourceUID}.dump";
            }
            MapKeyToSectorWindow mtsWin = new MapKeyToSectorWindow(main, tools);
            mtsWin.ShowDialog();
            main.RunMfoc(main.SelectedKeys, tools.TMPFILESOURCE_MFD);
        }
    }
}
