using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MCT_Windows
{
    /// <summary>
    /// Logique d'interaction pour WriteDumpWindow.xaml
    /// </summary>
    public partial class WriteDumpWindow : Window
    {
        Tools tools;
        MainWindow main;
        OpenFileDialog ofd = new OpenFileDialog();

        public WriteDumpWindow(MainWindow mainw, Tools t)
        {
            tools = t;
            main = mainw;
            InitializeComponent();
            Uri iconUri = new Uri("pack://application:,,,/Resources/MWT.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
            rbClone.IsChecked = true;
            ofd.Filter = "Dump Files|*.dump;*.mfd;*.dmp;*.img|All Files|*.*";
            ofd.InitialDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "dumps");
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
            if (rbFactoryFormat.IsChecked.HasValue && rbFactoryFormat.IsChecked.Value)
            {
                main.RunMifareClassicFormat();
                this.DialogResult = true;
                this.Close();
            }
            else if (rbClone.IsChecked.HasValue && rbClone.IsChecked.Value)
            {
                if (main.SelectedKeys.Any())
                {
                    main.RunNfcMfcClassic(TagAction.Clone, ckEnableBlock0Writing.IsChecked.HasValue && ckEnableBlock0Writing.IsChecked.Value, rbUseKeyA.IsChecked.HasValue && rbUseKeyA.IsChecked.Value);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("You need to select at least one key file");
                }
            }
        }

        private void btnSelectDump_Click(object sender, RoutedEventArgs e)
        {

            var dr = ofd.ShowDialog();
            if (dr.Value)
            {
                tools.TMPFILESOURCE_MFD = $"mfc_{ tools.mySourceUID}.dump";
            }
            MapKeyToSectorWindow mtsWin = new MapKeyToSectorWindow(main, tools);
            var ret = mtsWin.ShowDialog();
            if (ret.HasValue && ret.Value)
                main.RunMfoc(main.SelectedKeys, tools.TMPFILESOURCE_MFD);
        }

        private void rbFactoryFormat_Checked(object sender, RoutedEventArgs e)
        {
            btnWriteDump.Content = "Factory Format";
            btnSelectDump.Visibility = Visibility.Hidden;
            ckACs.Visibility = Visibility.Hidden;
            ckEnableBlock0Writing.Visibility = Visibility.Hidden;
            txtACsValue.Visibility = Visibility.Hidden;
            rbUseKeyA.Visibility = Visibility.Hidden;
            rbUseKeyB.Visibility = Visibility.Hidden;
        }

        private void rbClone_Checked(object sender, RoutedEventArgs e)
        {
            btnWriteDump.Content = "Write Dump";
            btnSelectDump.Visibility = Visibility.Visible;
            ckACs.Visibility = Visibility.Visible;
            ckEnableBlock0Writing.Visibility = Visibility.Visible;
            txtACsValue.Visibility = Visibility.Visible;
            rbUseKeyA.Visibility = Visibility.Visible;
            rbUseKeyB.Visibility = Visibility.Visible;
        }
    }
}
