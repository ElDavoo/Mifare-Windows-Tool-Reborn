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
            Uri iconUri = new Uri("pack://application:,,,/Resources/MWT.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
            mainw.StopScanTag();
            RefreshKeyFiles();
        }

        public void RefreshKeyFiles()
        {
            try
            {
                txtKeysPath.Text = tools.DefaultKeysPath;
                lstKeys.Items.Clear();
                foreach (var f in Directory.GetFiles(txtKeysPath.Text, "*.keys", SearchOption.AllDirectories))
                {
                    lstKeys.Items.Add(new MCTFile() { FileName = System.IO.Path.GetFileName(f), IsSelected = false });
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnEditKeyFile_Click(object sender, RoutedEventArgs e)
        {
            var selectedKeyFile = lstKeys.Items.OfType<MCTFile>().Where(k => k.IsSelected).FirstOrDefault();
            if (selectedKeyFile != null)
            {
                EditKeyFileWindow ekf = new EditKeyFileWindow(Main, tools, this, selectedKeyFile.FileName);
                ekf.ShowDialog();
            }
        }

        private void btnDeleteKeyFile_Click(object sender, RoutedEventArgs e)
        {
            var selectedKeyFile = lstKeys.Items.OfType<MCTFile>().Where(k => k.IsSelected).FirstOrDefault();
            if (selectedKeyFile != null)
            {
                System.IO.File.Delete(System.IO.Path.Combine(tools.DefaultKeysPath, selectedKeyFile.FileName));
                RefreshKeyFiles();
            }
        }

        private void btnNewKeyFile_Click(object sender, RoutedEventArgs e)
        {
            EditKeyFileWindow ekf = new EditKeyFileWindow(Main, tools, this);

            ekf.ShowDialog();
        }

        private void btnChangeDefaultKeyPath_Click(object sender, RoutedEventArgs e)
        {
            txtKeysPath.Text = tools.ChangeDefaultKeyPath();
            RefreshKeyFiles();
        }


        private void btnResetKeyPath_Click(object sender, RoutedEventArgs e)
        {
            txtKeysPath.Text = tools.ResetKeyPath();
            RefreshKeyFiles();
        }

    }
}
