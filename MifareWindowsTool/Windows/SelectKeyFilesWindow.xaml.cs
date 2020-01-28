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
    /// Logique d'interaction pour SelectKeyFilesWindow.xaml
    /// </summary>
    public partial class SelectKeyFilesWindow : Window
    {
        Tools tools = null;
        MainWindow Main = null;
        public SelectKeyFilesWindow(MainWindow mainw, Tools t)
        {
            tools = t;
            Main = mainw;
            InitializeComponent();
            mainw.StopScanTag();
            RefreshKeyFiles();
        }

        public void RefreshKeyFiles()
        {
            lstKeys.Items.Clear();
            foreach (var f in Directory.GetFiles("Keys", "*.keys", SearchOption.AllDirectories))
            {
                lstKeys.Items.Add(new Keys() { FileName = System.IO.Path.GetFileName(f), IsSelected = false });
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnEditKeyFile_Click(object sender, RoutedEventArgs e)
        {
            var selectedKeyFile = lstKeys.Items.OfType<Keys>().Where(k => k.IsSelected).FirstOrDefault();
            if (selectedKeyFile != null)
            {
                EditKeyFileWindow ekf = new EditKeyFileWindow(Main, tools, this, selectedKeyFile.FileName);
                ekf.ShowDialog();
            }
        }

        private void btnDeleteKeyFile_Click(object sender, RoutedEventArgs e)
        {
            var selectedKeyFile = lstKeys.Items.OfType<Keys>().Where(k => k.IsSelected).FirstOrDefault();
            if (selectedKeyFile != null)
            {
                File.Delete($"keys/{selectedKeyFile.FileName}");
                RefreshKeyFiles();
            }
        }

        private void btnNewKeyFile_Click(object sender, RoutedEventArgs e)
        {
            EditKeyFileWindow ekf = new EditKeyFileWindow(Main, tools, this);

            ekf.ShowDialog();
        }
    }
}
