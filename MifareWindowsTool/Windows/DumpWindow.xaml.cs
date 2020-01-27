using Microsoft.Win32;
using MifareWindowsTool.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

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
        byte[] bytesDataB = null;
        bool bConvertoAscii = true;
        Brush PaleBlueBrush = new SolidColorBrush(Color.FromArgb(255, (byte)0x60, (byte)0x8D, (byte)0x88));
        Brush VioletBrush = new SolidColorBrush(Color.FromArgb(255, (byte)0x95, (byte)0x33, (byte)0xF9));
        Tools Tools { get; set; }
        int split = 8;
        OpenFileDialog ofd = new OpenFileDialog();
        public DumpWindow(Tools t, string fileName, bool bCompareDumpsMode = false)
        {
            InitializeComponent();
            ofd.Filter = "Dump Files|*.dump;*.mfd|All Files|*.*";
            ofd.InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Tools = t;
            if (bCompareDumpsMode)
            {
                Title = "Compare Dumps";
                btnSaveDump.Visibility = Visibility.Hidden;
                btnShowAsAscii.Visibility = Visibility.Hidden;
                stkOpenDumps.Visibility = Visibility.Visible;
                stkInfos.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnSaveDump.Visibility = Visibility.Visible;
                stkOpenDumps.Visibility = Visibility.Collapsed;
                bytesDataA = File.ReadAllBytes(fileName);
                if (bytesDataA.Length == 1024) split = 4;
                ShowHex();
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
                LinesA.Insert(i, $"+Sector: {sector--}\r");

            for (int line = 0; line < LinesA.Count; line++)
            {
                if (line == 1)
                    txtOutput.AppendText(LinesA[line], VioletBrush, false);
                else if (line > 0 && (line % (split + 5 * cptOffset)) == 0)
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

        static List<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize) + "\r").ToList();
        }

        private void btnSaveDump_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Dump Files|*.dump;*.mfd";
            var dr = sfd.ShowDialog();
            if (dr.Value)
                File.WriteAllBytes(sfd.FileName, bytesDataA);

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnShowAsAscii_Click(object sender, RoutedEventArgs e)
        {
            if (bConvertoAscii)
            {
                btnShowAsAscii.Content = "Show as Hex";
                ShowAscii();
            }
            else
            {
                btnShowAsAscii.Content = "Show as ASCII";
                ShowHex();
            }
            bConvertoAscii = !bConvertoAscii;
        }

        private void ShowAscii()
        {
            string hex = BitConverter.ToString(bytesDataA).Replace("-", string.Empty);
            var ascii = Tools.ConvertHex(hex);
            LinesA = Split(ascii, 32);
            int sector = (LinesA.Count - 4) / 4;
            for (int i = LinesA.Count - 4; i >= 0; i -= 4)
                LinesA.Insert(i, $"+Sector: {sector--}\r");
            txtOutput.Document = new System.Windows.Documents.FlowDocument();
            txtOutput.AppendText(new string(LinesA.SelectMany(c => c).ToArray()));
        }
        private void BtnOpenDumpA_Click(object sender, RoutedEventArgs e)
        {

            var dr = ofd.ShowDialog();
            if (dr.Value)
            {
                btnOpenDumpA.Content = $"Open Dump A: {Path.GetFileNameWithoutExtension(ofd.FileName)}";
                bytesDataA = File.ReadAllBytes(ofd.FileName);
                ShowCompareDumps();
            }

        }

        private void BtnOpenDumpB_Click(object sender, RoutedEventArgs e)
        {
            var dr = ofd.ShowDialog();
            if (dr.Value)
            {
                btnOpenDumpB.Content = $"Open Dump B: {Path.GetFileNameWithoutExtension(ofd.FileName)}";
                bytesDataB = File.ReadAllBytes(ofd.FileName);
                ShowCompareDumps();
            }
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
                LinesA.Insert(i, $"\r+Sector:{sectorA--}\r");

            int sectorB = (LinesB.Count - split) / split;
            for (int i = LinesB.Count - split; i >= 0; i -= split)
                LinesB.Insert(i, "");

            for (int i = 0; i < Math.Max(LinesA.Count, LinesB.Count); i++)
            {
                if (i < LinesA.Count && !LinesA[i].StartsWith("\r+") || i < LinesB.Count && !LinesB[i].StartsWith("\r+"))
                {
                    if (i < LinesA.Count && i < LinesB.Count && LinesA[i] == LinesB[i])
                    {
                        txtOutput.AppendText("____________"); txtOutput.AppendText("Identical", Brushes.Lime); txtOutput.AppendText("_____________\r", Brushes.White);
                    }
                    else if (i < LinesB.Count && !string.IsNullOrWhiteSpace(LinesB[i]))
                    {
                        txtOutput.AppendText("____________"); txtOutput.AppendText("Different", Brushes.Red); txtOutput.AppendText("_____________\r", Brushes.White);

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


    }
}
