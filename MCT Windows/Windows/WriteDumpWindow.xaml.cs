using Microsoft.Win32;

using System.ComponentModel;
using System.Windows;

namespace MCT_Windows
{
    /// <summary>
    /// Logique d'interaction pour MapKeyToSectorWindow.xaml
    /// </summary>
    public partial class WriteDumpWindow : Window
    {
        Tools tools;
        MainWindow main;
        public WriteDumpWindow(MainWindow mainw, Tools t)
        {
            tools = t;
            main = mainw;
            InitializeComponent();
            
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

        }

        private void btnSelectDump_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Dump Files|*.dump";
            var dr = ofd.ShowDialog();
            if (dr.Value)
            {
                
            }
        }
    }
}
