using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MCT_Windows.Windows
{
    /// <summary>
    /// Logique d'interaction pour SelectDumpActionWindow.xaml
    /// </summary>
    public partial class SelectDumpActionWindow : Window
    {
        Tools tools = null;
        MainWindow Main = null;
        public SelectDumpActionWindow(MainWindow mainw, Tools t)
        {
            tools = t;
            Main = mainw;
            InitializeComponent();
            Uri iconUri = new Uri("pack://application:,,,/Resources/MWT.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
            mainw.StopScanTag();
            RefreshDumpFiles();
            
        }

        public void RefreshDumpFiles()
        {
            try
            {
                lstDumps.Items.Clear();
                foreach (var f in Directory.GetFiles("dumps", "*.*", SearchOption.AllDirectories))
                {
                    lstDumps.Items.Add(File() { FileName = System.IO.Path.GetFileName(f), IsSelected = false });
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

      

        private void btnEditDumpFile_Click(object sender, RoutedEventArgs e)
        {
            var selectedKeyFile = lstDumps.Items.OfType<File>().Where(k => k.IsSelected).FirstOrDefault();
            if (selectedKeyFile != null)
            {
                EditDumpWindow edw = new EditDumpWindow(selectedKeyFile.FileName);
                edw.ShowDialog();
            }
        }

        private void btnDeleteDumpFile_Click(object sender, RoutedEventArgs e)
        {
            var selectedKeyFile = lstDumps.Items.OfType<File>().Where(k => k.IsSelected).FirstOrDefault();
            if (selectedKeyFile != null)
            {
                System.IO.File.Delete($"dumps/{selectedKeyFile.FileName}");
                RefreshDumpFiles();
            }
        }

        private void btnShowDumpFile_Click(object sender, RoutedEventArgs e)
        {
            var selectedKeyFile = lstDumps.Items.OfType<File>().Where(k => k.IsSelected).FirstOrDefault();
            if (selectedKeyFile != null)
            {
                DumpWindow dw = new DumpWindow(tools, "dumps\\"+selectedKeyFile.FileName);
                dw.ShowDialog();
            }
        }
    }
}
