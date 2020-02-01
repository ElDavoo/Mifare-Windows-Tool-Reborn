using Microsoft.Win32;
using System;
using System.IO;
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
            ofd.Filter = MifareWindowsTool.Properties.Resources.DumpFileFilter;
            ofd.InitialDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "dumps");
            lblSrcDumpValue.Content = Path.GetFileName(tools.TMPFILESOURCE_MFD);
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
                await main.RunMifareClassicFormat();
                this.DialogResult = true;
                this.Close();
            }
            else if (rbClone.IsChecked.HasValue && rbClone.IsChecked.Value)
            {
                var de = DumpsExist();
                if (de == DumpExists.Both)
                {
                    TagType tt = TagType.Not0Writable;
                    if (rbtagGen1.IsChecked.Value) tt = TagType.UnlockedGen1;
                    else if (rbtagGen2.IsChecked.Value) tt = TagType.DirectCUIDgen2;

                    await main.RunNfcMfclassic(TagAction.Clone, ckEnableBlock0Writing.IsChecked.HasValue && ckEnableBlock0Writing.IsChecked.Value,
                          rbUseKeyA.IsChecked.HasValue && rbUseKeyA.IsChecked.Value, rbHaltOnError.IsChecked.HasValue && rbHaltOnError.IsChecked.Value, tt);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    if (de ==  DumpExists.Source)
                        MessageBox.Show(MifareWindowsTool.Properties.Resources.NeedSelectDumpKeyFileTarget);
                    else if (de == DumpExists.Target)
                        MessageBox.Show(MifareWindowsTool.Properties.Resources.NeedSelectDumpKeyFileSource);
                    else
                        if (de == DumpExists.None)
                        MessageBox.Show(MifareWindowsTool.Properties.Resources.NeedSelectDumpKeyFileSourceAndTarget);

                }
            }
        }

        DumpExists DumpsExist()
        {
            DumpExists de = DumpExists.None;
            if (System.IO.File.Exists("dumps\\" + tools.TMPFILESOURCE_MFD))
            {
                long fileLength = new System.IO.FileInfo("dumps\\" + tools.TMPFILESOURCE_MFD).Length;
                if (fileLength > 0) de = DumpExists.Source;

            }
            if (System.IO.File.Exists("dumps\\" + tools.TMPFILE_TARGETMFD))
            {
                long fileLength = new System.IO.FileInfo("dumps\\" + tools.TMPFILE_TARGETMFD).Length;
                if (fileLength > 0)
                    if (de == DumpExists.Source) de = DumpExists.Both; else de = DumpExists.Target;
            }
            return de;
        }

        private async void btnSelectDump_Click(object sender, RoutedEventArgs e)
        {

            var dr = ofd.ShowDialog();
            if (dr.Value)
            {
                tools.TMPFILESOURCE_MFD = Path.GetFileName(ofd.FileName);
                lblSrcDumpValue.Content = Path.GetFileName(tools.TMPFILESOURCE_MFD);
                lblTargetDumpValue.Content = Path.GetFileName(tools.TMPFILE_TARGETMFD);
            }
            if (string.IsNullOrWhiteSpace(lblTargetDumpValue.Content?.ToString()))
            {
                MapKeyToSectorWindow mtsWin = new MapKeyToSectorWindow(main, tools, MifareWindowsTool.Properties.Resources.UsedForTargetMapping);
                var ret = mtsWin.ShowDialog();
                if (ret.HasValue && ret.Value)
                    await main.RunMfoc(main.SelectedKeys, tools.TMPFILESOURCE_MFD, TagAction.ReadSource);
            }
        }

        private void rbFactoryFormat_Checked(object sender, RoutedEventArgs e)
        {
            btnWriteDump.Content = MifareWindowsTool.Properties.Resources.FactoryFormat;
            ShowHideElements(Visibility.Hidden);
        }

        private void rbClone_Checked(object sender, RoutedEventArgs e)
        {

            btnWriteDump.Content = MifareWindowsTool.Properties.Resources.StartCloning;
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
