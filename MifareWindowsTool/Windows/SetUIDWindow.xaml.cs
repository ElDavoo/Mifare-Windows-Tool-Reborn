using CliWrap;
using MifareWindowsTool.Common;
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
    /// Logique d'interaction pour SetUIDWindow.xaml
    /// </summary>
    public partial class SetUIDWindow : Window
    {
        Tools tools = null;
        MainWindow Main = null;
        public SetUIDWindow(MainWindow mainw, Tools t)
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

        private async void btnSetUID_Click(object sender, RoutedEventArgs e)
        {
            if (!txtnewUID.Text.Trim().OnlyHex() || txtnewUID.Text.Trim().Length != 8)
            {
                MessageBox.Show(MifareWindowsTool.Properties.Resources.InvalidUID);
                return;
            }
            await RunSetUidAsync(txtnewUID.Text.Trim());
        }
        private async Task RunSetUidAsync(string newUID)
        {
            var arguments = "";
            if (ckFormatTag.IsChecked.Value)
                arguments += "-f ";
            arguments += newUID;

            Main.logAppend($"nfc-mfsetuid {arguments}");
            var result = await Cli.Wrap(@"nfctools\\nfc-mfsetuid.exe").SetArguments(arguments)
   .SetStandardOutputCallback(l => Main.logAppend(l))
   .SetStandardErrorCallback(l => Main.logAppend(l))
   .ExecuteAsync();

            var exitCode = result.ExitCode;
            var stdOutput = result.StandardOutput;
            var stdErr = result.StandardError;
            var startTime = result.StartTime;
            var exitTime = result.ExitTime;
            var runTime = result.RunTime;


        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tools.myTargetUID))
            {
                var uid = await Main.RunNfcListAsync();

                if (string.IsNullOrWhiteSpace(uid))
                {
                    MessageBox.Show(MifareWindowsTool.Properties.Resources.NoTagDetectedOnReader);
                }
                else
                    txtOldUID.Text = uid;
            }

        }
    }
}
