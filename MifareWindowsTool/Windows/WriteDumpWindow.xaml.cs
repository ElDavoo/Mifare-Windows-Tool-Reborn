using Microsoft.Win32;

using MifareWindowsTool.Properties;

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
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
            ofd.Filter = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.DumpFileFilter));
            if (!string.IsNullOrWhiteSpace(tools.TMPFILESOURCEPATH_MFD) && System.IO.File.Exists(tools.TMPFILESOURCEPATH_MFD))
                ofd.InitialDirectory = Path.GetDirectoryName(tools.TMPFILESOURCEPATH_MFD);
            else
                ofd.InitialDirectory = tools.DefaultDumpPath;

            lblSrcDumpValue.Content = tools.TMPFILESOURCE_MFD;
            lblTargetDumpValue.Content = Path.GetFileName(tools.TMPFILE_TARGETMFD);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btnWriteDump_Click(object sender, RoutedEventArgs e)
        {
            if (rbFactoryFormat.IsChecked.HasValue && rbFactoryFormat.IsChecked.Value)
            {
                await main.RunMifareClassicFormatAsync(true);
                this.DialogResult = true;
                this.Close();
            }
            else if (rbClone.IsChecked.HasValue && rbClone.IsChecked.Value)
            {
                var de = DumpsExist(tools.TMPFILESOURCEPATH_MFD);
                if (de == DumpExists.Both)
                {
                    TagType tt = TagType.Not0Writable;
                    if (rbtagGen1.IsChecked.Value) tt = TagType.UnlockedGen1;
                    else if (rbtagGen2.IsChecked.Value) tt = TagType.DirectCUIDgen2;

                    await main.RunNfcMfclassicAsync(TagAction.Clone, ckEnableBlock0Writing.IsChecked.HasValue && ckEnableBlock0Writing.IsChecked.Value,
                          rbUseKeyA.IsChecked.HasValue && rbUseKeyA.IsChecked.Value, rbHaltOnError.IsChecked.HasValue && rbHaltOnError.IsChecked.Value, tt);

                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    if (de == DumpExists.Source)
                        MessageBox.Show(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.NeedSelectDumpKeyFileTarget)));
                    else if (de == DumpExists.Target)
                    {
                        MessageBox.Show(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.NeedSelectDumpKeyFileSource)));
                        await SelectDump();
                    }
                    else
                        if (de == DumpExists.None)
                        MessageBox.Show(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.NeedSelectDumpKeyFileSourceAndTarget)));

                }
            }
        }

        DumpExists DumpsExist(string sourcepath)
        {
            DumpExists de = DumpExists.None;
            if (System.IO.File.Exists(sourcepath))
            {
                long fileLength = new System.IO.FileInfo(sourcepath).Length;
                if (fileLength > 0) de = DumpExists.Source;

            }
            var path = Path.Combine(tools.DefaultDumpPath, tools.TMPFILE_TARGETMFD);
            if (System.IO.File.Exists(path))
            {
                long fileLength = new System.IO.FileInfo(path).Length;
                if (fileLength > 0)
                    if (de == DumpExists.Source) de = DumpExists.Both; else de = DumpExists.Target;
            }
            return de;
        }

        private async void btnSelectDump_Click(object sender, RoutedEventArgs e)
        {
            await SelectDump();
        }

        private async Task SelectDump()
        {
            var dr = ofd.ShowDialog();
            if (dr.Value)
            {
                tools.TMPFILESOURCEPATH_MFD = ofd.FileName;
                tools.TMPFILESOURCE_MFD = Path.GetFileName(ofd.FileName);
                lblSrcDumpValue.Content = tools.TMPFILESOURCE_MFD;
                lblTargetDumpValue.Content = Path.GetFileName(tools.TMPFILE_TARGETMFD);
            }
            if (string.IsNullOrWhiteSpace(lblTargetDumpValue.Content?.ToString()))
            {
                MapKeyToSectorWindow mtsWin = new MapKeyToSectorWindow(main, tools, Translate.Key(nameof(MifareWindowsTool.Properties.Resources.UsedForTargetMapping)), Translate.Key(nameof(MifareWindowsTool.Properties.Resources.TargetDump)));
                var ret = mtsWin.ShowDialog();
                if (ret.HasValue && ret.Value)
                    await main.RunMfocAsync(main.SelectedKeys, tools.TMPFILESOURCE_MFD, TagAction.ReadSource,
                         mtsWin.chkCustomProbeNb.IsChecked.HasValue && mtsWin.chkCustomProbeNb.IsChecked.Value ? mtsWin.udNbProbes.Value : 20, mtsWin.chkCustomProbeNb.IsChecked.HasValue && mtsWin.chkCustomProbeNb.IsChecked.Value ? mtsWin.udTolerance.Value : 20);
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
            rbtagGen1.IsChecked = true;
        }

        private void RbtagGen_Checked(object sender, RoutedEventArgs e)
        {

            ckEnableBlock0Writing.IsChecked = (rbtagGen1 != null && rbtagGen1.IsChecked.HasValue && rbtagGen1.IsChecked.Value);

        }

        private void CkEnableBlock0Writing_Unchecked(object sender, RoutedEventArgs e)
        {
            if (rbtagGen1.IsChecked.Value)
                rbtagGen0.IsChecked = true;
        }
    }
}
