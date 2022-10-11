using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MifareWindowsTool.Common;

namespace MCT_Windows.Windows
{
    /// <summary>
    /// Logique d'interaction pour EditDumpWindow.xaml
    /// </summary>
    public partial class EditDumpWindow : Window
    {
        public string FileNamePath { get; set; }

        public EditDumpWindow(string fileName)
        {

            InitializeComponent();

            he.FileName = fileName;
            Uri iconUri = new Uri("pack://application:,,,/Resources/MWT.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
          
            he.Foreground = Brushes.White;
            he.ForegroundSecondColor = Brushes.Orange;
            //to avoir freeze on HexEditor
            he.IsAutoRefreshOnResize = false;


        }
        private void btnSaveDump_Click(object sender, RoutedEventArgs e)
        {
            SaveDumpAs();
        }

        private void SaveDumpAs()
        {
            try
            {
                var sfd = DumpBase.CreateSaveDialog(fileName: Path.GetFileName(he.FileName));
                var dr = sfd.ShowDialog();
                if (dr.Value)
                {
                    var tagBytes = he.GetAllBytes();
                    he.CloseProvider();
                    System.IO.File.WriteAllBytes(sfd.FileName, tagBytes);

                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CloseEditor()
        {
            he.CloseProvider();
            he.Dispose();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                SaveDumpAs();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseEditor();
        }

        private void btnOpenDump_Click(object sender, RoutedEventArgs e)
        {
            var ofd = DumpBase.CreateOpenDialog();
            var dr = ofd.ShowDialog();
            if (dr.Value)
            {
                he.CloseProvider();
                he.FileName = ofd.FileName;

            }
        }
    }
}
