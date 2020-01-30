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
            ofd.Filter = MifareWindowsTool.Properties.Resources.DumpFileFilter;
            ofd.InitialDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "dumps");
            lblSrcDumpValue.Content = Path.GetFileName(tools.TMPFILESOURCE_MFD);
            lblTargetDumpValue.Content = Path.GetFileName(tools.TMPFILE_TARGETMFD);
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
                if (main.SelectedKeys.Any() || DumpsExist())
                {
                    main.RunNfcMfcClassic(TagAction.Clone, ckEnableBlock0Writing.IsChecked.HasValue && ckEnableBlock0Writing.IsChecked.Value,
                        rbUseKeyA.IsChecked.HasValue && rbUseKeyA.IsChecked.Value, rbHaltOnError.IsChecked.HasValue && rbHaltOnError.IsChecked.Value);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(MifareWindowsTool.Properties.Resources.NeedSelectDumpKeyFile);
                }
            }
        }

        bool DumpsExist()
        {
            if (System.IO.File.Exists("dumps\\" + tools.TMPFILESOURCE_MFD) && System.IO.File.Exists("dumps\\" + tools.TMPFILE_TARGETMFD))
            {
                long fileLength = new System.IO.FileInfo("dumps\\" + tools.TMPFILESOURCE_MFD).Length;
                if (fileLength > 0)
                {
                    fileLength = new System.IO.FileInfo("dumps\\" + tools.TMPFILE_TARGETMFD).Length;
                    return fileLength > 0;
                }
            }
            return false;
        }

        private void btnSelectDump_Click(object sender, RoutedEventArgs e)
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
                    main.RunMfoc(main.SelectedKeys, tools.TMPFILESOURCE_MFD);
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

        private void ShowHideElements(Visibility visibility)
        {
            btnSelectDump.Visibility = visibility;
            ckACs.Visibility = visibility;
            ckEnableBlock0Writing.Visibility = visibility;
            txtACsValue.Visibility = visibility;
            gbSelectKey.Visibility = visibility;
            gbSrcTgtDumps.Visibility = visibility;
            gbHaltTolerateError.Visibility = visibility;
        }
    }
}
