using Microsoft.Win32;
using MifareWindowsTool.Common;
using MifareWindowsTool.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MCT_Windows.Windows
{
    /// <summary>
    /// Logique d'interaction pour DumpWindow.xaml
    /// </summary>
    public partial class DumpWindow : Window
    {
        public List<string> LinesA { get; set; } = new List<string>();
        public List<string> LinesB { get; set; } = new List<string>();
        byte[] bytesDataA = null;
        public string dFileName { get; set; }
        byte[] bytesDataB = null;
        bool bConvertoAscii = true;
        Brush PaleBlueBrush = new SolidColorBrush(Color.FromArgb(255, (byte)0x60, (byte)0x8D, (byte)0x88));
        Brush VioletBrush = new SolidColorBrush(Color.FromArgb(255, (byte)0x95, (byte)0x33, (byte)0xF9));
        Tools Tools { get; }
        int split = 8;
        OpenFileDialog ofd = new OpenFileDialog();
        SaveFileDialog sfd = new SaveFileDialog();

        public DumpWindow(Tools t, string fileName, bool bCompareDumpsMode = false)
        {
            try
            {
                InitializeComponent();
                Uri iconUri = new Uri("pack://application:,,,/Resources/MWT.ico", UriKind.RelativeOrAbsolute);
                this.Icon = BitmapFrame.Create(iconUri);
                ofd.Filter = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.DumpFileFilter));
                var initialDumpDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "dumps");
                ofd.InitialDirectory = initialDumpDir;
                sfd.Filter = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.DumpFileFilter));
                sfd.InitialDirectory = initialDumpDir;
                Tools = t;
                dFileName = fileName;

                if (bCompareDumpsMode)
                {
                    Title = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.CompareDumps));
                    btnSaveDump.Visibility = Visibility.Hidden;
                    btnShowAsAscii.Visibility = Visibility.Hidden;
                    stkOpenDumps.Visibility = Visibility.Visible;
                    stkInfos.Visibility = Visibility.Collapsed;
                    btnEdit.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Title += " " + Path.GetFileName(dFileName);
                    btnEdit.Visibility = Visibility.Visible;
                    btnSaveDump.Visibility = Visibility.Visible;
                    stkOpenDumps.Visibility = Visibility.Collapsed;
                    bytesDataA = System.IO.File.ReadAllBytes(fileName);
                    if (bytesDataA.Length == 1024) split = 4;
                    ShowHex();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void ShowHex()
        {
            txtOutput.Document = new System.Windows.Documents.FlowDocument();
            string hex = BitConverter.ToString(bytesDataA).Replace("-", string.Empty);
            int cptOffset = 0;
            LinesA = Split(hex, 32);
            int sector = (LinesA.Count - split) / split;
            for (int i = LinesA.Count - split; i >= 0; i -= split)
                LinesA.Insert(i, $"+{Translate.Key(nameof(MifareWindowsTool.Properties.Resources.Sector))}: {sector--}\r");

            for (int line = 0; line < LinesA.Count; line++)
            {
                if (line == 1)
                    txtOutput.AppendText(LinesA[line], VioletBrush, false);
                else if (line > 0 && (line % (split + (split + 1) * cptOffset)) == 0)
                {
                    for (int c = 0; c < LinesA[line].Length; c++)
                    {
                        if (c <= 11) txtOutput.AppendText(LinesA[line][c].ToString(), Brushes.Lime);
                        else if (c > 11 && c <= 19) txtOutput.AppendText(LinesA[line][c].ToString(), Brushes.Orange);
                        else txtOutput.AppendText(LinesA[line][c].ToString(), Brushes.Green);

                    }
                    cptOffset += 1;
                }
                else
                    txtOutput.AppendText(LinesA[line], Brushes.White, false);

            }



        }
        public byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length).Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
        }
        private List<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize) + "\r").ToList();
        }

        private void btnSaveDump_Click(object sender, RoutedEventArgs e)
        {

            var dr = sfd.ShowDialog();
            if (dr.Value)
                System.IO.File.WriteAllBytes(sfd.FileName, bytesDataA);

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
                ShowAscii();
            }
            else
            {
                btnShowAsAscii.Content = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.ShowAsASCII));
                ShowHex();
            }
            bConvertoAscii = !bConvertoAscii;
        }

        private void ShowAscii()
        {
            string hexString = BitConverter.ToString(bytesDataA).Replace("-", string.Empty);
            var ascii = hexString.HexStrToAscii();
            LinesA = Split(ascii, 32);
            int sector = (LinesA.Count - 4) / 4;
            for (int i = LinesA.Count - 4; i >= 0; i -= 4)
                LinesA.Insert(i, $"+{Translate.Key(nameof(MifareWindowsTool.Properties.Resources.Sector))}: {sector--}\r");
            txtOutput.Document = new System.Windows.Documents.FlowDocument();
            txtOutput.AppendText(new string(LinesA.SelectMany(c => c).ToArray()));
        }
        private void BtnOpenDumpA_Click(object sender, RoutedEventArgs e)
        {
            var fileName = OpenDump(ref bytesDataA);
            if (!string.IsNullOrWhiteSpace(fileName))
                btnOpenDumpA.Content = $"{Translate.Key(nameof(MifareWindowsTool.Properties.Resources.OpenDump))} A: {Path.GetFileNameWithoutExtension(fileName)}";

        }
        private void BtnOpenDumpB_Click(object sender, RoutedEventArgs e)
        {
            var fileName = OpenDump(ref bytesDataB);
            if (!string.IsNullOrWhiteSpace(fileName))
                btnOpenDumpB.Content = $"{Translate.Key(nameof(MifareWindowsTool.Properties.Resources.OpenDump))} B: {Path.GetFileNameWithoutExtension(fileName)}";
        }

        private string OpenDump(ref byte[] bytes)
        {
            var fileName = "";
            var dr = ofd.ShowDialog();
            if (dr.Value)
            {
                fileName = ofd.FileName;
                FileInfo fi = new FileInfo(ofd.FileName);
                if (fi.Length < 1024 || fi.Length > 4096)
                {
                    MessageBox.Show(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.InvalidDumpFile)));
                    return "";
                }

                bytes = System.IO.File.ReadAllBytes(fileName);
                ShowCompareDumps();
            }
            return fileName;
        }



        private void ShowCompareDumps()
        {

            if (bytesDataA == null || bytesDataB == null) return;

            Mouse.OverrideCursor = Cursors.Wait;
            txtOutput.Document = new System.Windows.Documents.FlowDocument();

            string hexA = BitConverter.ToString(bytesDataA).Replace("-", string.Empty);
            string hexB = BitConverter.ToString(bytesDataB).Replace("-", string.Empty);

            LinesA = Split(hexA, 32);
            LinesB = Split(hexB, 32);
            if (bytesDataA.Length == 1024) split = 4;

            int sectorA = (LinesA.Count - split) / split;
            for (int i = LinesA.Count - split; i >= 0; i -= split)
                LinesA.Insert(i, $"\r+{Translate.Key(nameof(MifareWindowsTool.Properties.Resources.Sector))}:{sectorA--}\r");

            int sectorB = (LinesB.Count - split) / split;
            for (int i = LinesB.Count - split; i >= 0; i -= split)
                LinesB.Insert(i, "");

            for (int i = 0; i < Math.Max(LinesA.Count, LinesB.Count); i++)
            {
                if (i < LinesA.Count && !LinesA[i].StartsWith("\r+") || i < LinesB.Count && !LinesB[i].StartsWith("\r+"))
                {
                    if (i < LinesA.Count && i < LinesB.Count && LinesA[i] == LinesB[i])
                    {
                        txtOutput.AppendText("____________"); txtOutput.AppendText(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.Identical)), Brushes.Lime); txtOutput.AppendText("_____________\r", Brushes.White);
                    }
                    else if (i < LinesB.Count && !string.IsNullOrWhiteSpace(LinesB[i]))
                    {
                        txtOutput.AppendText("____________"); txtOutput.AppendText(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.Different)), Brushes.Red); txtOutput.AppendText("_____________\r", Brushes.White);

                        for (int j = 0; j < LinesA[i].Count(); j++)
                        {
                            if (j == 0)
                            {
                                txtOutput.AppendText("  ");
                            }
                            if (LinesA[i][j] != LinesB[i][j])
                                txtOutput.AppendText("v", Brushes.Red);
                            else
                                txtOutput.AppendText(" ", Brushes.White);
                        }
                        txtOutput.AppendText("\r", Brushes.White);
                    }
                }
                if (i < LinesA.Count)
                    txtOutput.AppendText((!string.IsNullOrWhiteSpace(LinesA[i]) && !LinesA[i].StartsWith("\r+") ? "A:" : "") + LinesA[i], LinesA[i].StartsWith("\r+") ? PaleBlueBrush : Brushes.White, LinesA[i].StartsWith("\r+") ? true : false);
                if (i < LinesB.Count)
                    txtOutput.AppendText((!string.IsNullOrWhiteSpace(LinesB[i]) && !LinesB[i].StartsWith("\r+") ? "B:" : "") + LinesB[i], LinesB[i].StartsWith("\r+") ? PaleBlueBrush : Brushes.White, LinesB[i].StartsWith("\r+") ? true : false);
            }
            Mouse.OverrideCursor = null;

        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var edw = new EditDumpWindow(Tools, dFileName);
            Mouse.OverrideCursor = Cursors.Wait;

            edw.Show();


        }
    }
}
