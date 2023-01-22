using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using MifareWindowsTool.Common;
using MifareWindowsTool.Properties;


namespace MCT_Windows
{
    /// <summary>
    /// Logique d'interaction pour WriteDumpWindow.xaml
    /// </summary>
    public partial class WriteDumpWindow : Window
    {
        Tools Tools;
        MainWindow main;
        public WriteDumpWindow(MainWindow mainw, Tools t)
        {
            Tools = t;
            main = mainw;
            InitializeComponent();
            Uri iconUri = new Uri("pack://application:,,,/Resources/MWT.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
            rbClone.IsChecked = true;
            lblSrcDumpValue.Content = Tools.SourceBinaryDump.DumpFileName != null ? $"{Tools.SourceBinaryDump.DumpFileName.Replace("_", "__")} ({Tools.SourceBinaryDump.StrDumpType})" : "";
            lblTargetDumpValue.Content = Tools.TargetBinaryDump.DumpFileName != null ? $"{Tools.TargetBinaryDump.DumpFileName.Replace("_", "__")} ({Tools.TargetBinaryDump.StrDumpType})" : "";

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btnWriteDump_Click(object sender, RoutedEventArgs e)
        {
            if (rbFactoryFormat.IsChecked.HasValue && rbFactoryFormat.IsChecked.Value)
            {
                await main.RunSetUidAsync("", true);
                //this.DialogResult = true;
                this.Close();
            }
            else if (rbClone.IsChecked.HasValue && rbClone.IsChecked.Value)
            {
                if (!Tools.TargetBinaryDump.IsValid)
                {
                    await SelectTargetDump(true);
                }
                if (!Tools.SourceBinaryDump.IsValid)
                {
                    await SelectSourceDump(true);
                }

                if (Tools.SourceBinaryDump.IsValid && Tools.TargetBinaryDump.IsValid)
                {
                    if (Tools.SourceBinaryDump.StrDumpUID == Tools.TargetBinaryDump.StrDumpUID)
                    {
                        var dr = MessageBox.Show(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.WantSourceAndCopyTheSame)), Translate.Key(nameof(MifareWindowsTool.Properties.Resources.SourceAndCopyAreTheSame)), MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);
                        if (dr != MessageBoxResult.Yes) return;
                    }
                    await WriteDumpAction();
                }
            }
        }
        private async Task WriteDumpAction()
        {
            TagType tt = TagType.Not0Writable;
            if (rbtagGen1.IsChecked.Value) tt = TagType.UnlockedGen1;
            else if (rbtagGen2.IsChecked.Value) tt = TagType.DirectCUIDgen2;

            bool bWritePartialBlocks = ckWritePartialBlocks.IsChecked.HasValue && ckWritePartialBlocks.IsChecked.Value;
            var forceStartBlockValue = bWritePartialBlocks && upDownStartBlock.Value.HasValue ? upDownStartBlock.Value.Value : 0;
            var forceEndBlockValue = bWritePartialBlocks && upDownEndBlock.Value.HasValue ? upDownEndBlock.Value.Value : -1;

            await main.RunNfcMfclassicAsync(ckEnableBlock0Writing.IsChecked.HasValue && ckEnableBlock0Writing.IsChecked.Value,
                  rbUseKeyA.IsChecked.HasValue && rbUseKeyA.IsChecked.Value, rbHaltOnError.IsChecked.HasValue && rbHaltOnError.IsChecked.Value
                  , txtACsValue.Text, string.Empty, forceStartBlockValue, forceEndBlockValue, 0);

            await main.RunNfcListAsync();

            this.DialogResult = true;
            this.Close();
        }
        private async Task SelectSourceDump(bool showWarning = true)
        {
            var msg = showWarning ? Translate.Key(nameof(MifareWindowsTool.Properties.Resources.NeedSelectDumpKeyFileSource)) : string.Empty;

            var dump = Tools.SourceBinaryDump;
            SelectDump(ref dump, lblSrcDumpValue, Translate.Key(nameof(MifareWindowsTool.Properties.Resources.SelectDumpForSourceTag)));
            Tools.SourceBinaryDump = dump;
            if (Tools.SourceBinaryDump.IsValid) showWarning = false;
            if (showWarning && !string.IsNullOrEmpty(msg)) MessageBox.Show(msg);// msg if showwarning
        }
        private async Task SelectTargetDump(bool showWarning = true)
        {
            var msg = showWarning ? Translate.Key(nameof(MifareWindowsTool.Properties.Resources.NeedSelectDumpKeyFileTarget)) : string.Empty;

            var dump = Tools.TargetBinaryDump;
            if (showWarning) MessageBox.Show(msg);
            SelectDump(ref dump, lblTargetDumpValue, Translate.Key(nameof(MifareWindowsTool.Properties.Resources.SelectDumpForTargetTag)));
            Tools.TargetBinaryDump = dump;
            if (string.IsNullOrEmpty(Tools.TargetBinaryDump.DumpFileFullName))
            {
                MapKeyToSectorWindow mtsWin = new MapKeyToSectorWindow(main, Tools, Translate.Key(nameof(MifareWindowsTool.Properties.Resources.UsedForTargetMapping)),
                    Translate.Key(nameof(MifareWindowsTool.Properties.Resources.TargetDump)));
                var ret = mtsWin.ShowDialog();
                if (ret.HasValue && ret.Value)
                    await main.RunMfocAsync(main.SelectedKeys, Tools.TargetBinaryDump.DumpFileName, TagAction.ReadTarget,
                         mtsWin.chkCustomProbeNb.IsChecked.HasValue && mtsWin.chkCustomProbeNb.IsChecked.Value ? mtsWin.udNbProbes.Value : 20,
                         mtsWin.chkCustomProbeNb.IsChecked.HasValue && mtsWin.chkCustomProbeNb.IsChecked.Value ? mtsWin.udTolerance.Value : 20);
            }
        }

        private void SelectDump(ref IDump dump, System.Windows.Controls.Label lbl, string dlgTitle)
        {
            var ofd = DumpBase.CreateOpenDialog(title: dlgTitle);
            if (System.IO.File.Exists(dump.DumpFileFullName)) ofd.InitialDirectory = Path.GetDirectoryName(dump.DumpFileFullName);
            var dr = ofd.ShowDialog();
            if (dr.Value)
            {
                dump = DumpBase.OpenExistingDump(ofd.FileName);
                if (dump == null)
                {
                    lbl.Content = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.InvalidDumpFile));
                    return;
                }
                lbl.Content = $"{dump.DumpFileName.Replace("_", "__")} ({dump.StrDumpType})";
            }

        }

        private void rbFactoryFormat_Checked(object sender, RoutedEventArgs e)
        {
            btnWriteDump.Content = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.FactoryFormat));
            ShowHideElements(Visibility.Hidden);
        }

        private void rbClone_Checked(object sender, RoutedEventArgs e)
        {

            btnWriteDump.Content = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.StartCloning));
            ShowHideElements(Visibility.Visible);
        }

        private void ShowHideElements(Visibility vis)
        {
            gbWriteOptions.Visibility = vis;
            gbSrcTgtDumps.Visibility = vis;
        }

        private void CkEnableBlock0Writing_Checked(object sender, RoutedEventArgs e)
        {
            if (rbtagGen2 != null && rbtagGen2.IsChecked.HasValue && !rbtagGen2.IsChecked.Value)
                rbtagGen1.IsChecked = true;
        }

        private void RbtagGen_Checked(object sender, RoutedEventArgs e)
        {

            ckEnableBlock0Writing.IsChecked = (rbtagGen1 != null && rbtagGen1.IsChecked.HasValue && rbtagGen1.IsChecked.Value || rbtagGen2 != null && rbtagGen2.IsChecked.HasValue && rbtagGen2.IsChecked.Value);

        }

        private void CkEnableBlock0Writing_Unchecked(object sender, RoutedEventArgs e)
        {
            //if (rbtagGen1.IsChecked.Value || rbtagGen2.IsChecked.Value)
            //    rbtagGen0.IsChecked = true;
        }

        private void btnTagGensInfo_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://blogmotion.fr/internet/securite/gen1-gen2-gen3-nfc-mifare-1k-18004");
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ckEnableBlock0Writing.IsChecked = await main.RunDetectChineseMagicCardAsync();
        }
        private async void btnSelectsourceDump_Click(object sender, RoutedEventArgs e)
        {
            await SelectSourceDump(false);
        }
        private async void btnSelectTargetDump_Click(object sender, RoutedEventArgs e)
        {
            await SelectTargetDump(false);
        }

        private void ckACs_Checked(object sender, RoutedEventArgs e)
        {
            var ck = sender as CheckBox;
            if (txtACsValue == null) return;
            var defaultACL = ck.IsChecked.HasValue && ck.IsChecked.Value;
            if (defaultACL)
            {
                txtACsValue.Text = Tools.DefaultAccessConditions;
                txtACsValue.IsReadOnly = true;
            }
        }

        private void ckWritePartialBlocks_Checked(object sender, RoutedEventArgs e)
        {
            var ck = sender as CheckBox;
            var active = ck.IsChecked.HasValue && ck.IsChecked.Value;
            upDownStartBlock.IsEnabled = active;
            upDownEndBlock.IsEnabled = active;
        }

        private void btnACLCalculator_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://calc.gmss.ru/Mifare1k/");
        }

        private void upDownStartBlock_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var upDown = sender as Xceed.Wpf.Toolkit.IntegerUpDown;
            if (upDown == null || upDownEndBlock == null) return;

            if (upDown.Value > upDownEndBlock.Value)
            {
                upDown.Value = upDownEndBlock.Value;
                e.Handled = true;
            }
        }

        private void upDownEndBlock_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var upDown = sender as  Xceed.Wpf.Toolkit.IntegerUpDown;
            if (upDown == null || upDownStartBlock == null) return;
            if (upDown.Value < upDownStartBlock.Value)
            {
                upDown.Value = upDownStartBlock.Value;
                e.Handled = true;
            }
        }
    }
}
