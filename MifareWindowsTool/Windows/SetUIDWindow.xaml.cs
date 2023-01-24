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
        string currentUID = "";
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

            if (!txtnewUID.Text.Trim().OnlyHex() || (txtnewUID.Text.Trim().Length != 8)
                && (txtnewUID.Text.Trim().Length != 14) && (txtnewUID.Text.Trim().Length != 32))
            {
                MessageBox.Show(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.InvalidUID)));
                return;
            }
            string oldUID = await Main.RunNfcListAsync();
            Main.ProcessCTS = new System.Threading.CancellationTokenSource();
            await Main.RunSetUidAsync(txtnewUID.Text.Trim(), ckFormatTag.IsChecked.Value);
            string newUID = await Main.RunNfcListAsync();
           
            MessageBox.Show(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.Finished)
                +Environment.NewLine
                +$"UID:{oldUID} --> {newUID}"),"UID");
            this.Close();
        }


        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            var exeFile = "nfc-mfsetuid.exe";
            if (!Tools.CheckNfcToolsFolder(exeFile)) return;
            if (Tools.TargetBinaryDump == null || string.IsNullOrWhiteSpace(Tools.TargetBinaryDump.StrDumpUID))
            {
                Main.ScanCTS = new System.Threading.CancellationTokenSource();

                currentUID = await Main.RunNfcListAsync();
            }
            else
            {
                currentUID = Tools.TargetBinaryDump.StrDumpUID;
            }
            if (string.IsNullOrWhiteSpace(currentUID))
            {
                var message = Tools.nfcDeviceFound ? Translate.Key(nameof(MifareWindowsTool.Properties.Resources.NoTagDetectedOnReader)) :
                    Translate.Key(nameof(MifareWindowsTool.Properties.Resources.BadgeReaderAcr122NotFound));
                MessageBox.Show(message);
                this.Close();
            }
            else
                txtOldUID.Text = currentUID;

            btnSetUID.IsEnabled = !string.IsNullOrWhiteSpace(currentUID);
        }
    }
}
