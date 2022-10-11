using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MifareWindowsTool.Common;
using MifareWindowsTool.Properties;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;
using RadioButton = System.Windows.Controls.RadioButton;

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
                txtKeysPath.Text = DumpBase.DefaultKeysPath;
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
            var rb = GetRbFromInnerButton(sender as Button);
            rb.IsChecked = true;
            var selectedKeyFile = lstKeys.Items.OfType<MCTFile>().Where(k => k.IsSelected).FirstOrDefault();
            if (selectedKeyFile == null) return;
            EditKeyFileWindow ekf = new EditKeyFileWindow(Main, this, selectedKeyFile.FileName);
            ekf.ShowDialog();
        }
        private void btnRenameKeyFile_Click(object sender, RoutedEventArgs e)
        {
            var rb = GetRbFromInnerButton(sender as Button);
            rb.IsChecked = true;
            var selectedKeyFile = lstKeys.Items.OfType<MCTFile>().Where(k => k.IsSelected).FirstOrDefault();
            if (selectedKeyFile == null) return;
            txtRenameKeyFile.Text = Path.GetFileNameWithoutExtension(selectedKeyFile.FileName);
            stRenameKeyfile.Visibility = Visibility.Visible;
        }

        private RadioButton GetRbFromInnerButton(Button button)
        {
            return ((button.Parent as StackPanel).Parent as Border).Parent as RadioButton;
        }

        private void btnDeleteKeyFile_Click(object sender, RoutedEventArgs e)
        {

            var rb = GetRbFromInnerButton(sender as Button);
            rb.IsChecked = true;
            var selectedKeyFile = lstKeys.Items.OfType<MCTFile>().Where(k => k.IsSelected).FirstOrDefault();
            if (selectedKeyFile == null) return;
            var msg = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.ConfirmDeleteFile));
            var caption = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.Warning));
            var dr = MessageBox.Show(msg, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (dr != MessageBoxResult.Yes) return;
            System.IO.File.Delete(System.IO.Path.Combine(DumpBase.DefaultKeysPath, selectedKeyFile.FileName));
            RefreshKeyFiles();
        }
        private void btnNewKeyFile_Click(object sender, RoutedEventArgs e)
        {
            EditKeyFileWindow ekf = new EditKeyFileWindow(Main, this);

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


        private void txtRenameKeyFile_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return) ValidateRenameFileName();
        }

        private void ValidateRenameFileName()
        {
            if (txtRenameKeyFile.Text.Length > 0)
            {
                var invalidChars = txtRenameKeyFile.Text.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0;
                var fileExists = File.Exists(System.IO.Path.Combine(DumpBase.DefaultKeysPath, txtRenameKeyFile.Text));
                if (invalidChars || fileExists)
                {
                    var msg = fileExists ? Translate.Key(nameof(MifareWindowsTool.Properties.Resources.NameAlreadyExisting)) : Translate.Key(nameof(MifareWindowsTool.Properties.Resources.InvalidCharsInFileName));
                    MessageBox.Show(msg);
                    return;
                }

                var selectedKeyFile = lstKeys.Items.OfType<MCTFile>().Where(k => k.IsSelected).FirstOrDefault();
                if (selectedKeyFile == null) return;
                var oldPath = System.IO.Path.Combine(DumpBase.DefaultKeysPath, selectedKeyFile.FileName);
                var newPath = System.IO.Path.Combine(DumpBase.DefaultKeysPath, txtRenameKeyFile.Text);
                if (!newPath.EndsWith(".keys")) newPath += ".keys";
                System.IO.File.Move(oldPath, newPath);
                stRenameKeyfile.Visibility = Visibility.Collapsed;
                txtRenameKeyFile.Clear();
                RefreshKeyFiles();
            }
        }

        private void btnRenameOK_Click(object sender, RoutedEventArgs e)
        {
            ValidateRenameFileName();
        }
    }
}
