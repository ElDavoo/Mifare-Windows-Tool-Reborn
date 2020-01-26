using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace MCT_Windows.Windows
{
    /// <summary>
    /// Logique d'interaction pour DumpWindow.xaml
    /// </summary>
    public partial class EditKeyFileWindow : Window
    {
        public List<string> Lines { get; set; } = new List<string>();
        string Data = "";
        MainWindow Main = null;
        string FileName = "";
        SelectKeyFilesWindow Skf = null;
        Tools Tools { get; set; }
        public EditKeyFileWindow(MainWindow main, Tools t, SelectKeyFilesWindow skf, string fileName = "")
        {
            InitializeComponent();
            Main = main;
            Skf = skf;
            Tools = t;
            if (!string.IsNullOrWhiteSpace(FileName))
            {
                FileName = $"keys/{fileName}";
                Data = File.ReadAllText(FileName);
            }
               
            else
                Data = "";
            ShowKeys();
        }

        private void ShowKeys()
        {

            Lines = Data.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
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
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Key Files|*.keys";
                var dr = sfd.ShowDialog();
                if (!dr.Value) return;
                FileName = sfd.FileName;
            }

            Data = txtOutput.Text;
            File.WriteAllText(FileName, Data);

        }
    }
}
