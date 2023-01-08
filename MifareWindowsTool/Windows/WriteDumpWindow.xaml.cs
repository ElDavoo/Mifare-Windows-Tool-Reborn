using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Windows;
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
                await main.RunSetUidAsync("",true);
                //this.DialogResult = true;
                this.Close();
            }
            else if (rbClone.IsChecked.HasValue && rbClone.IsChecked.Value)
            {
                if (!Tools.SourceBinaryDump.IsValid || !Tools.TargetBinaryDump.IsValid)
                {
                    await SelectSourceOrTargetDump();

                }

                if (Tools.SourceBinaryDump.IsValid && Tools.TargetBinaryDump.IsValid)
                {
                    if (Tools.SourceBinaryDump.DumpFileFullName == Tools.TargetBinaryDump.DumpFileFullName)
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

            await main.RunNfcMfclassicAsync(TagAction.Clone, ckEnableBlock0Writing.IsChecked.HasValue && ckEnableBlock0Writing.IsChecked.Value,
                  rbUseKeyA.IsChecked.HasValue && rbUseKeyA.IsChecked.Value, rbHaltOnError.IsChecked.HasValue && rbHaltOnError.IsChecked.Value, tt);

            this.DialogResult = true;
            this.Close();
        }

        private async Task SelectSourceOrTargetDump(bool showWarning = true)
        {
            var msg = "";
            if (!Tools.TargetBinaryDump.IsValid) // need target
            {
                var dump = Tools.TargetBinaryDump;
                if (showWarning) MessageBox.Show(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.NeedSelectDumpKeyFileTarget)));
                SelectDump(ref dump, lblTargetDumpValue, Translate.Key(nameof(MifareWindowsTool.Properties.Resources.SelectDumpForTargetTag)));
                Tools.TargetBinaryDump = dump;
                if (string.IsNullOrEmpty(Tools.TargetBinaryDump.DumpFileFullName))
                {
                    MapKeyToSectorWindow mtsWin = new MapKeyToSectorWindow(main, Tools, Translate.Key(nameof(MifareWindowsTool.Properties.Resources.UsedForTargetMapping)), Translate.Key(nameof(MifareWindowsTool.Properties.Resources.TargetDump)));
                    var ret = mtsWin.ShowDialog();
                    if (ret.HasValue && ret.Value)
                        await main.RunMfocAsync(main.SelectedKeys, Tools.TargetBinaryDump.DumpFileName, TagAction.ReadTarget,
                             mtsWin.chkCustomProbeNb.IsChecked.HasValue && mtsWin.chkCustomProbeNb.IsChecked.Value ? mtsWin.udNbProbes.Value : 20, mtsWin.chkCustomProbeNb.IsChecked.HasValue && mtsWin.chkCustomProbeNb.IsChecked.Value ? mtsWin.udTolerance.Value : 20);
                }
            }
            else //target valid => just need source
            {
                if (showWarning)
                {
                    msg = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.NeedSelectDumpKeyFileSource));
                    //!det.HasValue && !des.HasValue ? Translate.Key(nameof(MifareWindowsTool.Properties.Resources.NeedSelectDumpKeyFileSourceAndTarget)) : "";
                }
                var dump = Tools.SourceBinaryDump;
                SelectDump(ref dump, lblSrcDumpValue, Translate.Key(nameof(MifareWindowsTool.Properties.Resources.SelectDumpForSourceTag)));
                Tools.SourceBinaryDump = dump;
            }
           

            if (Tools.SourceBinaryDump.IsValid) showWarning = false;
            if (showWarning && !string.IsNullOrEmpty(msg)) MessageBox.Show(msg);// msg if showwarning
        }
        private async void btnSelectDump_Click(object sender, RoutedEventArgs e)
        {
            await SelectSourceOrTargetDump(false);
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
    }
}
