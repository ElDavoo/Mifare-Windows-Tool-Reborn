using MifareWindowsTool.Properties;

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
        public MapKeyToSectorWindow(MainWindow mainw, Tools t, string lblContent, string titleContent)
        {
            tools = t;
            main = mainw;
            InitializeComponent();
            if (!string.IsNullOrWhiteSpace(lblContent))
            {
                lblChooseKeyFile.Content += $"\n{lblContent}";
            }
            Title += $" {titleContent}";
            Uri iconUri = new Uri("pack://application:,,,/Resources/MWT.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
            foreach (var f in Directory.GetFiles(tools.DefaultKeysPath, "*.keys", SearchOption.AllDirectories))
            {
                var fName = System.IO.Path.GetFileName(f);
                ucLK.lstKeys.Items.Add(new MCTFile()
                {
                    FileName = fName,
                    IsSelected = fName == "std.keys"
                });
            }

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnStartMappingAndReadTag_Click(object sender, RoutedEventArgs e)
        {
            main.SelectedKeys = ucLK.lstKeys.Items.OfType<MCTFile>().Where(c => c.IsSelected).ToList();
            if (main.SelectedKeys.Any())
            {
                this.DialogResult = true;
                this.Close();
            }
            else
                MessageBox.Show(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.NeedToSelectOneKeyFile)));
        }

        private void default_rpt(object sender, ProgressChangedEventArgs e)
        {
            main.LogAppend((string)e.UserState);

        }

    }
}
