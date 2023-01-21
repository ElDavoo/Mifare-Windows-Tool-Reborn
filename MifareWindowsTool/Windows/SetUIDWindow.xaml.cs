using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

using CliWrap;

using MifareWindowsTool.Common;
using MifareWindowsTool.Properties;

namespace MCT_Windows.Windows
{
    /// <summary>
    /// Logique d'interaction pour SetUIDWindow.xaml
    /// </summary>
    public partial class SetUIDWindow : Window
    {
        Tools Tools = null;
        MainWindow Main = null;
        public SetUIDWindow(MainWindow mainw, Tools t)
        {
            Tools = t;
            Main = mainw;
            InitializeComponent();
            Uri iconUri = new Uri("pack://application:,,,/Resources/MWT.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btnSetUID_Click(object sender, RoutedEventArgs e)
        {

            if (!txtnewUID.Text.Trim().OnlyHex() || (txtnewUID.Text.Trim().Length != 8 && txtnewUID.Text.Trim().Length != 32))
            {
                MessageBox.Show(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.InvalidUID)));
                return;
            }
            Main.ProcessCTS = new System.Threading.CancellationTokenSource();
            await Main.RunSetUidAsync(txtnewUID.Text.Trim(), ckFormatTag.IsChecked.Value);
            MessageBox.Show(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.Finished)));
            this.Close();
        }


        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var uid = "";
            var exeFile = "nfc-mfsetuid.exe";
            if (!Tools.CheckNfcToolsFolder(exeFile)) return;
            if (Tools.TargetBinaryDump == null || string.IsNullOrWhiteSpace(Tools.TargetBinaryDump.StrDumpUID))
            {
                Main.ScanCTS = new System.Threading.CancellationTokenSource();

                uid = await Main.RunNfcListAsync();
            }
            else
            {
                uid = Tools.TargetBinaryDump.StrDumpUID;
            }
            if (string.IsNullOrWhiteSpace(uid))
            {
                var message = Tools.nfcDeviceFound ? Translate.Key(nameof(MifareWindowsTool.Properties.Resources.NoTagDetectedOnReader)) :
                    Translate.Key(nameof(MifareWindowsTool.Properties.Resources.BadgeReaderAcr122NotFound));
                MessageBox.Show(message);
                this.Close();
            }
            else
                txtOldUID.Text = uid;

            btnSetUID.IsEnabled = !string.IsNullOrWhiteSpace(uid);
        }
    }
}
