using Microsoft.Win32;

using MifareWindowsTool.Common;
using MifareWindowsTool.Properties;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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
        int sectorCounter = 0;
        int blockCountInSector = 4;
        const int blocksize = 16;
        public int sectorCount { get; set; } = 16;
        Dump dumpA { get; set; } = new Dump();
        Dump dumpB { get; set; } = new Dump();
        DumpConverter converter = new DumpConverter();
        public string dFileName { get; set; }
        bool bConvertoAscii = true;
        int split = 8;
        Brush PaleBlueBrush = new SolidColorBrush(Color.FromArgb(255, (byte)0x60, (byte)0x8D, (byte)0x88));
        Brush VioletBrush = new SolidColorBrush(Color.FromArgb(255, (byte)0x95, (byte)0x33, (byte)0xF9));
        Tools Tools { get; }

        OpenFileDialog ofd = new OpenFileDialog();
        SaveFileDialog sfd = new SaveFileDialog();
        public bool CompareDumpsMode { get; private set; }

        public DumpWindow(Tools t, string fileName, bool bCompareDumpsMode = false)
        {
            try
            {
                InitializeComponent();
                CompareDumpsMode = bCompareDumpsMode;
                Uri iconUri = new Uri("pack://application:,,,/Resources/MWT.ico", UriKind.RelativeOrAbsolute);
                this.Icon = BitmapFrame.Create(iconUri);
                ofd.Filter = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.DumpFileFilter));
                var initialDumpDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "dumps");
                ofd.InitialDirectory = initialDumpDir;
                sfd.Filter = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.DumpFileFilter));
                sfd.InitialDirectory = initialDumpDir;
                Tools = t;
                dFileName = fileName;


            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }

        }

        private void SetSectorCount(int dataLength)
        {
            //2293 mifare 1K MCT dump (text file)
            if (dataLength == 1024 || dataLength == 2293)
            {
                sectorCount = 16;
                rb1K.IsChecked = true;
                split = 4;

            }
            else if (dataLength == 320)
            {
                sectorCount = 5;
                rbmini.IsChecked = true;
            }
            else if (dataLength == 2048)
            {
                sectorCount = 32;
                rb2K.IsChecked = true;
            }
            else if (dataLength == 4096)
            {
                sectorCount = 40;
                rb4K.IsChecked = true;
            }

        }

        public byte[][] BufferSplit(byte[] buffer, int blockSize)
        {
            byte[][] blocks = new byte[(buffer.Length + blockSize - 1) / blockSize][];

            for (int i = 0, j = 0; i < blocks.Length; i++, j += blockSize)
            {
                blocks[i] = new byte[Math.Min(blockSize, buffer.Length - j)];
                Array.Copy(buffer, j, blocks[i], 0, blocks[i].Length);
            }

            return blocks;
        }
        public byte[] ConvertToBinaryDump(string text)
        {
            var Lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();

            List<byte> binaryOutput = new List<byte>();

            foreach (var line in Lines.Where(l => !l.Contains("Sector") && !l.Contains("Secteur")))
            {
                binaryOutput.AddRange(StringToByteArray(line));
            }
            return binaryOutput.ToArray();
        }

        private void ShowAscii()
        {
            sectorCounter = 0;
            txtOutput.Document = new System.Windows.Documents.FlowDocument();
            byte[][] chunks = BufferSplit(dumpA.BinArray, blocksize);
            for (int i = 0; i < chunks.GetLength(0); i++)
            {
                if (sectorCount == 40 && i > 128)
                {
                    //if tag contains 40 sectors, the first 32 sectors contain 4 blocks and the last 8 sectors contain 16 blocks.
                    blockCountInSector = 16;
                }
                else
                    blockCountInSector = 4;
                string hexString = BitConverter.ToString(chunks[i]).Replace("-", string.Empty);
                var strAscii = hexString.HexStrToAscii();
                if (i % blockCountInSector == 0)
                {
                    txtOutput.AppendText($"{Translate.Key(nameof(MifareWindowsTool.Properties.Resources.Sector))}: {sectorCounter++}{Environment.NewLine}", Brushes.LightBlue);
                }
                if (i == 0)
                    txtOutput.AppendText($"{Environment.NewLine}", Brushes.White);
                txtOutput.AppendText($"{strAscii}{Environment.NewLine}", Brushes.White);
            }

        }
        private void ShowHex()
        {
            if (txtOutput == null) return;
            sectorCounter = 0;
            txtOutput.Document = new System.Windows.Documents.FlowDocument();

            byte[][] chunks = BufferSplit(dumpA.BinArray, blocksize);

            for (int i = 0; i < chunks.GetLength(0); i++)
            {
                if (sectorCount == 40 && i > 128)
                {
                    //if tag contains 40 sectors, the first 32 sectors contain 4 blocks and the last 8 sectors contain 16 blocks.
                    blockCountInSector = 16;
                }
                else
                    blockCountInSector = 4;
                string strBlock = $"{BitConverter.ToString(chunks[i]).Replace("-", string.Empty)}{Environment.NewLine}";
                if (i % blockCountInSector == 0)
                {
                    txtOutput.AppendText($"{Translate.Key(nameof(MifareWindowsTool.Properties.Resources.Sector))}: {sectorCounter++}{Environment.NewLine}", Brushes.LightBlue);
                }
                if (i == 0)
                    txtOutput.AppendText($"{Environment.NewLine}{strBlock}", VioletBrush, false);
                else if ((i % blockCountInSector - (blockCountInSector - 1)) == 0)
                {
                    for (int c = 0; c < blocksize * 2; c++)
                    {
                        if (c <= 11) txtOutput.AppendText(strBlock[c].ToString(), Brushes.Lime);
                        else if (c > 11 && c <= 19) txtOutput.AppendText(strBlock[c].ToString(), Brushes.Orange);
                        else txtOutput.AppendText(strBlock[c].ToString(), Brushes.Green);

                    }
                    txtOutput.AppendText(Environment.NewLine, Brushes.White);
                }
                else
                    txtOutput.AppendText(strBlock, Brushes.White);

            }

        }
        public byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length).Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), blocksize)).ToArray();
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
                System.IO.File.WriteAllBytes(sfd.FileName, dumpA.BinArray);

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


        private void BtnOpenDumpA_Click(object sender, RoutedEventArgs e)
        {
            var fileName = OpenDump(true);
            if (!string.IsNullOrWhiteSpace(fileName))
                btnOpenDumpA.Content = $"{Translate.Key(nameof(MifareWindowsTool.Properties.Resources.OpenDump))} A: {Path.GetFileNameWithoutExtension(fileName)}";

        }
        private void BtnOpenDumpB_Click(object sender, RoutedEventArgs e)
        {
            var fileName = OpenDump(false);
            if (!string.IsNullOrWhiteSpace(fileName))
                btnOpenDumpB.Content = $"{Translate.Key(nameof(MifareWindowsTool.Properties.Resources.OpenDump))} B: {Path.GetFileNameWithoutExtension(fileName)}";
        }

        private string OpenDump(bool isA)
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

                var inputFileType = converter.CheckDump(fileName);
                if (inputFileType == FileType.Text)
                {
                    if (isA)
                    {
                        lblInfosA.Content = "A:text dump";
                        dumpA = converter.ConvertToBinaryDump();
                    }
                    else
                    {
                        lblInfosB.Content = "B:text dump";
                        dumpB = converter.ConvertToBinaryDump();
                    }


                }
                else
                {
                    if (isA)
                    {
                        lblInfosA.Content = "A:binary dump";
                        dumpA.BinaryOutput = System.IO.File.ReadAllBytes(fileName).ToList();
                    }
                    else
                    {
                        lblInfosB.Content = "B:binary dump";
                        dumpB.BinaryOutput = System.IO.File.ReadAllBytes(fileName).ToList();
                    }

                }

                ShowCompareDumps();
            }
            return fileName;
        }



        private void ShowCompareDumps()
        {

            if (dumpA.BinArray == null || dumpB.BinArray == null) return;

            Mouse.OverrideCursor = Cursors.Wait;
            txtOutput.Document = new System.Windows.Documents.FlowDocument();

            string hexA = BitConverter.ToString(dumpA.BinArray).Replace("-", string.Empty);
            string hexB = BitConverter.ToString(dumpB.BinArray).Replace("-", string.Empty);

            dumpA.Lines = Split(hexA, blocksize * 2);
            dumpB.Lines = Split(hexB, blocksize * 2);
            if (dumpA.BinArray.Length == 1024) split = 4;

            int sectorA = (dumpA.LinesCount - split) / split;
            for (int i = dumpA.LinesCount - split; i >= 0; i -= split)
                dumpA.Lines.Insert(i, $"\r+{Translate.Key(nameof(MifareWindowsTool.Properties.Resources.Sector))}:{sectorA--}\r");

            int sectorB = (dumpB.LinesCount - split) / split;
            for (int i = dumpB.LinesCount - split; i >= 0; i -= split)
                dumpB.Lines.Insert(i, "");

            for (int i = 0; i < Math.Max(dumpA.LinesCount, dumpB.LinesCount); i++)
            {
                if (i < dumpA.LinesCount && !dumpA.Lines[i].StartsWith("\r+") || i < dumpB.LinesCount && !dumpB.Lines[i].StartsWith("\r+"))
                {
                    if (i < dumpA.LinesCount && i < dumpB.LinesCount && dumpA.Lines[i] == dumpB.Lines[i])
                    {
                        txtOutput.AppendText("____________"); txtOutput.AppendText(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.Identical)), Brushes.Lime); txtOutput.AppendText("_____________\r", Brushes.White);
                    }
                    else if (i < dumpB.LinesCount && !string.IsNullOrWhiteSpace(dumpB.Lines[i]))
                    {
                        txtOutput.AppendText("____________"); txtOutput.AppendText(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.Different)), Brushes.Red); txtOutput.AppendText("_____________\r", Brushes.White);

                        if (i < dumpA.LinesCount)
                            for (int j = 0; j < dumpA.Lines[i].Count(); j++)
                            {
                                if (j == 0)
                                {
                                    txtOutput.AppendText("  ");
                                }
                                if (dumpA.Lines[i][j] != dumpB.Lines[i][j])
                                    txtOutput.AppendText("v", Brushes.Red);
                                else
                                    txtOutput.AppendText(" ", Brushes.White);
                            }
                        txtOutput.AppendText("\r", Brushes.White);
                    }
                }
                if (i < dumpA.LinesCount)
                    txtOutput.AppendText((!string.IsNullOrWhiteSpace(dumpA.Lines[i]) && !dumpA.Lines[i].StartsWith("\r+") ? "A:" : "") + dumpA.Lines[i], dumpA.Lines[i].StartsWith("\r+") ? PaleBlueBrush : Brushes.White, dumpA.Lines[i].StartsWith("\r+") ? true : false);
                if (i < dumpB.LinesCount)
                    txtOutput.AppendText((!string.IsNullOrWhiteSpace(dumpB.Lines[i]) && !dumpB.Lines[i].StartsWith("\r+") ? "B:" : "") + dumpB.Lines[i], dumpB.Lines[i].StartsWith("\r+") ? PaleBlueBrush : Brushes.White, dumpB.Lines[i].StartsWith("\r+") ? true : false);
            }
            Mouse.OverrideCursor = null;

        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var edw = new EditDumpWindow(Tools, dFileName);
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
            }
            else
            {
                Title += " " + Path.GetFileName(dFileName);
                btnEdit.Visibility = Visibility.Visible;
                btnSaveDump.Visibility = Visibility.Visible;
                stkOpenDumps.Visibility = Visibility.Collapsed;


                dumpA.BinaryOutput = System.IO.File.ReadAllBytes(dFileName).ToList();

                string strFirstBlock = BitConverter.ToString(dumpA.BinArray.Take(16).ToArray()).Replace("-", string.Empty);
                if (strFirstBlock.Contains("536563746F72")) //(hex)536563746F72 => (text)-> 'Sector'
                {
                    System.Windows.MessageBox.Show(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.thisismctdumpfile))
                         , "MCT Dump --> MWT Dump", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);

                    dumpA.TextOutput = System.IO.File.ReadAllText(dFileName);
                    dumpA = converter.ConvertToBinaryDump();

                }

                SetSectorCount(dumpA.BinArray.Length);

                ShowHex();
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var rb = sender as RadioButton;
            switch (rb.Name)
            {
                case "rbmini": sectorCount = 5; ShowHex(); break;
                case "rb1K": sectorCount = 16; ShowHex(); break;
                case "rb2K": sectorCount = 32; ShowHex(); break;
                case "rb4K": sectorCount = 40; ShowHex(); break;
            }
        }


    }
}
