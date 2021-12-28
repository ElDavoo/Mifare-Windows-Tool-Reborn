using Microsoft.Win32;

using MifareWindowsTool.Properties;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MCT_Windows.Windows
{
    /// <summary>
    /// Logique d'interaction pour EditKeyFileWindow.xaml
    /// </summary>
    public partial class EditKeyFileWindow : Window
    {
        public List<string> Lines { get; set; } = new List<string>();
        string Data = "";
        MainWindow Main = null;
        string FileName = "";
        SelectKeyFilesWindow Skf = null;
        SaveFileDialog sfd = new SaveFileDialog();
        Tools Tools { get; set; }
        public EditKeyFileWindow(MainWindow main, Tools t, SelectKeyFilesWindow skf, string fileName = "")
        {
            InitializeComponent();
            Uri iconUri = new Uri("pack://application:,,,/Resources/MWT.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
            Main = main;
            Skf = skf;
            Tools = t;
            sfd.Filter = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.KeyFilesFilter));
            sfd.InitialDirectory = Tools.DefaultKeysPath;

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                FileName = Path.Combine(Tools.DefaultKeysPath, fileName);
                Data = System.IO.File.ReadAllText(FileName);
            }

            else
                Data = "";
            ShowKeys();
        }

        private void ShowKeys()
        {

            Lines = Data.ToUpper().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var d = new string(Lines.SelectMany(c => c).ToArray());

            txtOutput.Text = d;

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Skf.RefreshKeyFiles();
            this.Close();
        }

        private void btnSaveKeyFile_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrWhiteSpace(FileName))
            {

                var dr = sfd.ShowDialog();
                if (!dr.Value) return;
                FileName = sfd.FileName;
            }

            Data = txtOutput.Text;
            System.IO.File.WriteAllText(FileName, Data);

            Skf.RefreshKeyFiles();
            this.Close();
        }
    }
}
