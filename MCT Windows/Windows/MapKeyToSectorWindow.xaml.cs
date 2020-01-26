using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MCT_Windows
{
    /// <summary>
    /// Logique d'interaction pour MapKeyToSectorWindow.xaml
    /// </summary>
    public partial class MapKeyToSectorWindow : Window
    {
        Tools tools;
        MainWindow main;
        public MapKeyToSectorWindow(MainWindow mainw, Tools t)
        {
            tools = t;
            main = mainw;
            InitializeComponent();
            foreach (var f in Directory.GetFiles("Keys", "*.keys", SearchOption.AllDirectories))
            {
                ucLK.lstKeys.Items.Add(new Keys() { FileName = System.IO.Path.GetFileName(f), IsSelected = false });
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnStartMappingAndReadTag_Click(object sender, RoutedEventArgs e)
        {
            main.SelectedKeys = ucLK.lstKeys.Items.OfType<Keys>().Where(c => c.IsSelected).ToList();
            if (main.SelectedKeys.Any())
                this.Close();
            else
                MessageBox.Show("You need to select at least one key file");
        }


        private void default_rpt(object sender, ProgressChangedEventArgs e)
        {
            main.logAppend((string)e.UserState);

        }


    }
}
