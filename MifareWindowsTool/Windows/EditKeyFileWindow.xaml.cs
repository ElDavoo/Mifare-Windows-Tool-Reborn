using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using MifareWindowsTool.Common;
using MifareWindowsTool.Properties;

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
       
        public EditKeyFileWindow(MainWindow main, SelectKeyFilesWindow skf, string fileName = "")
        {
            InitializeComponent();
            Uri iconUri = new Uri("pack://application:,,,/Resources/MWT.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
            Main = main;
            Skf = skf;

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                FileName = Path.Combine(DumpBase.DefaultKeysPath, fileName);
                Data = System.IO.File.ReadAllText(FileName);
                this.Title += $" ({fileName})";
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
                var dlg = DumpBase.CreateSaveDialog(title: Translate.Key(nameof(MifareWindowsTool.Properties.Resources.SaveKeyFile)), filter: Translate.Key(nameof(MifareWindowsTool.Properties.Resources.KeyFilesFilter)), initialDir: DumpBase.DefaultKeysPath);
                var dr = dlg.ShowDialog();
                if (!dr.Value) return;
                FileName = dlg.FileName;
            }

            Data = txtOutput.Text;
            System.IO.File.WriteAllText(FileName, Data);

            Skf.RefreshKeyFiles();
            this.Close();
        }
    }
}
