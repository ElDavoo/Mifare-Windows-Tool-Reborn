using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using Microsoft.Win32;

using MifareWindowsTool.Common;
using MifareWindowsTool.Properties;

namespace MCT_Windows.Windows
{
    /// <summary>
    /// Logique d'interaction pour DumpWindow.xaml
    /// </summary>
    public partial class DumpWindow : Window
    {

        IDump dumpA;
        IDump dumpB;
        bool bConvertoAscii = true;
        public bool CompareDumpsMode { get; private set; }

        public DumpWindow(string fileName = null)
        {
            try
            {
                InitializeComponent();
                CompareDumpsMode = fileName == null;
                Uri iconUri = new Uri("pack://application:,,,/Resources/MWT.ico", UriKind.RelativeOrAbsolute);
                this.Icon = BitmapFrame.Create(iconUri);
                if (!CompareDumpsMode)
                {
                    stRbBlocks.IsEnabled = true;
                    dumpA = DumpBase.OpenExistingDump(fileName);
                    ShowDump(dumpA, lblInfosA);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }

        }

        private void SetSectorCount(IDump dmp)
        {
            switch (dmp.CardType)
            {
                case CardType.MifareMini: rbmini.IsChecked = true; break;
                case CardType.NTag213: rbNTag213.IsChecked = true; break;
                case CardType.NTag215: rbNTag213.IsChecked = true; break;
                case CardType.NTag216: rbNTag213.IsChecked = true; break;
                case CardType.Mifare1K: rb1K.IsChecked = true; break;
                case CardType.Mifare2K: rb2K.IsChecked = true; break;
                case CardType.Mifare4K: rb4K.IsChecked = true; break;

            }
        }

        private void btnSaveDump_Click(object sender, RoutedEventArgs e)
        {
            var dlg = DumpBase.CreateSaveDialog(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.SaveDump)));
            var dr = dlg.ShowDialog();
            if (dr.HasValue && dr.Value) System.IO.File.WriteAllBytes(dlg.FileName, dumpA.DumpData.HexData.ToArray());
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btnShowAsAscii_Click(object sender, RoutedEventArgs e)
        {
            if (bConvertoAscii)
            {
                btnShowAsAscii.Content = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.ShowAsHex));
                dumpA.ShowAscii(txtOutput);
            }
            else
            {
                btnShowAsAscii.Content = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.ShowAsASCII));
                txtKeys.Text = dumpA.ShowHexAndAddDumpKeys(txtOutput);
            }
            bConvertoAscii = !bConvertoAscii;
        }
        private void BtnOpenDumpA_Click(object sender, RoutedEventArgs e)
        {
            OpenDump(ref dumpA, btnOpenDumpA, lblInfosA, "A");
        }

        private void BtnOpenDumpB_Click(object sender, RoutedEventArgs e)
        {
            OpenDump(ref dumpB, btnOpenDumpB, lblInfosB, "B");
        }

        private void ShowDump(IDump dmp, Label lbl)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            txtKeys.Text = dmp.ShowHexAndAddDumpKeys(txtOutput);
            var content = $"{dmp.DumpFileName} ({dmp.StrDumpType})";
            lbl.Content = content.Replace("_", "__");
            if (!CompareDumpsMode)
            {
                Title += " " + content;
                SetSectorCount(dmp);
                //txtOutput.AppendText(string.Join("\r\n", dumpA.DumpData.LstTextData));
            }
            Mouse.OverrideCursor = null;
        }

        private void OpenDump(ref IDump dmp, Button btn, Label lbl, string whichDump)
        {
            try
            {
                var txtOpenDump = $"{Translate.Key(nameof(MifareWindowsTool.Properties.Resources.OpenDump))} {whichDump}";
                Mouse.OverrideCursor = Cursors.Wait;
                btn.Content = txtOpenDump;
                dmp = DumpBase.OpenCreateDump(out bool canceled, txtOpenDump);
                if (dmp == null)
                {
                    if (!canceled) MessageBox.Show(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.InvalidDumpFile)));
                    return;
                }
                var content = $"{dmp.DumpFileName.Replace("_", "__")} ({dmp.StrDumpType})";
                lbl.Content = content;
                if (CompareDumpsMode)
                {
                    if (dumpA != null && dumpB != null) ShowCompareDumps();
                    else if (dmp != null)
                    {
                        ShowDump(dmp, lbl);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }


        }
        private void ShowCompareDumps()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            dumpA.CompareTo(dumpB, txtOutput);
            Mouse.OverrideCursor = null;

        }
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var edw = new EditDumpWindow(dumpA.DumpFileFullName);
            Mouse.OverrideCursor = Cursors.Wait;
            edw.Show();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Height = SystemParameters.PrimaryScreenHeight * 0.8;
            if (CompareDumpsMode)
            {
                Title = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.CompareDumps));
                btnSaveDump.Visibility = Visibility.Hidden;
                btnShowAsAscii.Visibility = Visibility.Hidden;
                stkOpenDumps.Visibility = Visibility.Visible;
                stkInfos.Visibility = Visibility.Collapsed;
                btnEdit.Visibility = Visibility.Collapsed;
                spKeys.Visibility = Visibility.Collapsed;
                btnSaveDumpKeys.Visibility = Visibility.Collapsed;
                btnAppendDumpKeys.Visibility = Visibility.Collapsed;
            }
            else
            {

                btnEdit.Visibility = Visibility.Visible;
                btnSaveDump.Visibility = Visibility.Visible;
                stkOpenDumps.Visibility = Visibility.Collapsed;
                spKeys.Visibility = Visibility.Visible;
                btnSaveDumpKeys.Visibility = Visibility.Visible;
                btnAppendDumpKeys.Visibility = Visibility.Visible;

            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (dumpA is null) return;
            dumpA.ShowHexAndAddDumpKeys(txtOutput);
        }
        private void btnAppendDumpKeys_Click(object sender, RoutedEventArgs e)
        {
            if (!HasDumpKeys()) return;
            var dlg = DumpBase.CreateOpenDialog(filter: Translate.Key(nameof(MifareWindowsTool.Properties.Resources.KeyFilesFilter)), initialDir: DumpBase.DefaultKeysPath, fileName: Path.GetFileNameWithoutExtension(dumpA.DumpFileFullName));
            SetdialogAndSaveOrAppendKeys(dlg, (fn) => System.IO.File.AppendAllLines(fn, dumpA.DumpKeys.Distinct()));
        }
        private void btnSaveDumpKeys_Click(object sender, RoutedEventArgs e)
        {
            if (!HasDumpKeys()) return;
            var dlg = DumpBase.CreateSaveDialog(filter: Translate.Key(nameof(MifareWindowsTool.Properties.Resources.KeyFilesFilter)), initialDir: DumpBase.DefaultKeysPath, fileName: Path.GetFileNameWithoutExtension(dumpA.DumpFileFullName));
            SetdialogAndSaveOrAppendKeys(dlg, (fn) => System.IO.File.WriteAllText(fn, string.Join(Environment.NewLine, dumpA.DumpKeys.Distinct())));
        }

        private void SetdialogAndSaveOrAppendKeys(FileDialog dlg, Action<string> fileAction)
        {
            var dr = dlg.ShowDialog();
            if (dr.HasValue && dr.Value)
                fileAction(dlg.FileName);
        }
        private bool HasDumpKeys()
        {
            var hasKeys = dumpA.DumpKeys.Any();
            if (!hasKeys) MessageBox.Show(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.NothingToSave)));
            return hasKeys;
        }

    }
}
