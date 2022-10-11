using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using MifareWindowsTool.Common;
using MifareWindowsTool.DumpClasses;
namespace MCT_Windows.Windows
{
    /// <summary>
    /// Logique d'interaction pour SelectToolWindow.xaml
    /// </summary>
    public partial class SelectOutputDumpTypeWindow : Window
    {

        private IDump SourceDump { get; }
        private IDump TargetDump { get; set; }
        public SelectOutputDumpTypeWindow()
        {
            InitializeComponent();
            Uri iconUri = new Uri("pack://application:,,,/Resources/MWT.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
        }
        public SelectOutputDumpTypeWindow(IDump sourceDump) : this()
        {
            SourceDump = sourceDump;

            rbMwt.Visibility = sourceDump is IDumpMWT ? Visibility.Collapsed : Visibility.Visible;
            rbMwt.IsChecked = IsVisible;
            rbFlipper.Visibility = sourceDump is IDumpFlipper ? Visibility.Collapsed : Visibility.Visible;
            rbFlipper.IsChecked = IsVisible;
            rbMct.Visibility = sourceDump is IDumpMct ? Visibility.Collapsed : Visibility.Visible;
            rbMct.IsChecked = IsVisible;
            lblInputDumpType.Content = $"{sourceDump.StrDumpType} dump detected";
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var rb = sender as RadioButton;
            if (!rb.IsChecked.HasValue || !rb.IsChecked.Value) return;
            var radioButtons = LogicalTreeHelper.GetChildren(rbOutputDumps).OfType<RadioButton>();

            if (btnConvertDump != null)
                btnConvertDump.IsEnabled = radioButtons.Any(x => (bool)x.IsChecked);
        }
        private void btnConvertDump_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                if (rbMct.IsChecked.HasValue && rbMct.IsChecked.Value) //Convert from MWT or flipper to Mct (mifare classic)
                {
                    TargetDump = new DumpMct();
                }
                else if (rbFlipper.IsChecked.HasValue && rbFlipper.IsChecked.Value) //Convert from MWT or MCT to Flipper
                {
                    TargetDump = new DumpFlipper();
                }
                else if (rbMwt.IsChecked.HasValue && rbMwt.IsChecked.Value) //Convert from Flipper or MCT to MWT (libnfc) binary
                {
                    TargetDump = new DumpMWT();
                }
                TargetDump.ConvertFrom(SourceDump, ckFillEmptyWithDefault.IsChecked.HasValue && ckFillEmptyWithDefault.IsChecked.Value);

                var sfd = DumpBase.CreateSaveDialog(fileName: SourceDump.DumpFileNameWithoutExt + TargetDump.DefaultDumpExtension);

                var ret = sfd.ShowDialog();
                if (!ret.HasValue || !ret.Value) return;
                if (TargetDump is IDumpMWT)
                {
                    File.WriteAllBytes(sfd.FileName, TargetDump.DumpData.HexData.ToArray());
                }
                else if (TargetDump is IDumpFlipper || TargetDump is IDumpMct)
                {
                    File.WriteAllText(sfd.FileName, TargetDump.DumpData.TextData);
                }

                System.Windows.MessageBox.Show($"Dump conversion from {SourceDump.StrDumpType} to {TargetDump.StrDumpType}: Done");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);

            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

    }
}
