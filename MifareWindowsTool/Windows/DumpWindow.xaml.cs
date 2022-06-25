using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        int sectorCounter = 0;
        int blockCountInSector = 4;
        const int blocksize = 16;
        public int sectorCount { get; set; } = 16;
        Dump dumpA { get; set; } = new Dump();
        Dump dumpB { get; set; } = new Dump();
        DumpConverter converter = new DumpConverter();
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
                ofd.InitialDirectory = t.DefaultDumpPath;
                sfd.Filter = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.DumpFileFilter));
                sfd.InitialDirectory = t.DefaultDumpPath;
                Tools = t;
                dumpA.FileName = fileName;


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
            dumpA.Keys.Clear();
            dumpA.Keys.Add($"# dump keys from {Path.GetFileNameWithoutExtension(dumpA.FileName)} added on {DateTime.Now.Date:MMMM dd yyyy}");
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
                        if (c <= 11)
                        {
                            txtOutput.AppendText(strBlock[c].ToString(), Brushes.Lime);

                        }
                        else if (c > 11 && c <= 19 && strBlock.Length > c) txtOutput.AppendText(strBlock[c].ToString(), Brushes.Orange);
                        else if (strBlock.Length > c)
                        {
                            txtOutput.AppendText(strBlock[c].ToString(), Brushes.Green);

                        }


                    }
                    //add keys to dump
                    var keyA = strBlock.Substring(0, 12);
                    if (!dumpA.Keys.Contains(keyA))
                        dumpA.Keys.Add(keyA);
                    var keyB = strBlock.Length >= 32 ? strBlock.Substring(20, 12) : "";
                    if (keyB != "" && !dumpA.Keys.Contains(keyB))
                        dumpA.Keys.Add(keyB);

                    txtOutput.AppendText(Environment.NewLine, Brushes.White);
                }
                else
                    txtOutput.AppendText(strBlock, Brushes.White);

            }
            txtKeys.Text = string.Join(Environment.NewLine, dumpA.Keys);

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
            sfd.Filter = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.DumpFileFilter));
            sfd.InitialDirectory = Tools.DefaultDumpPath;
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
            dumpA.FileName = OpenDump(true);
            if (!string.IsNullOrWhiteSpace(dumpA.FileName))
                btnOpenDumpA.Content = $"{Translate.Key(nameof(MifareWindowsTool.Properties.Resources.OpenDump))} A: {Path.GetFileNameWithoutExtension(dumpA.FileName)}";

        }
        private void BtnOpenDumpB_Click(object sender, RoutedEventArgs e)
        {
            dumpB.FileName = OpenDump(false);
            if (!string.IsNullOrWhiteSpace(dumpB.FileName))
                btnOpenDumpB.Content = $"{Translate.Key(nameof(MifareWindowsTool.Properties.Resources.OpenDump))} B: {Path.GetFileNameWithoutExtension(dumpB.FileName)}";
        }

        private string OpenDump(bool isA)
        {

            var dr = ofd.ShowDialog();
            if (dr.Value)
            {
                FileInfo fi = new FileInfo(ofd.FileName);
                if (fi.Length < 1024 || fi.Length > 4096)
                {
                    MessageBox.Show(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.InvalidDumpFile)));
                    return "";
                }


                var inputFileType = converter.CheckDump(ofd.FileName);
                if (inputFileType == FileType.Text)
                {
                    if (isA)
                    {
                        lblInfosA.Content = "A:text dump";
                        dumpA = converter.ConvertToBinaryDump(dumpA);
                    }
                    else
                    {
                        lblInfosB.Content = "B:text dump";
                        dumpB = converter.ConvertToBinaryDump(dumpB);
                    }


                }
                else if (inputFileType == FileType.Binary)
                {
                    if (isA)
                    {
                        lblInfosA.Content = "A:binary dump";
                        dumpA.BinaryOutput = System.IO.File.ReadAllBytes(ofd.FileName).ToList();
                    }
                    else
                    {
                        lblInfosB.Content = "B:binary dump";
                        dumpB.BinaryOutput = System.IO.File.ReadAllBytes(ofd.FileName).ToList();
                    }

                }

                ShowCompareDumps();
            }
            return ofd.FileName;
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
            var edw = new EditDumpWindow(Tools, dumpA.FileName);
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
                Title += " " + Path.GetFileName(dumpA.FileName);
                btnEdit.Visibility = Visibility.Visible;
                btnSaveDump.Visibility = Visibility.Visible;
                stkOpenDumps.Visibility = Visibility.Collapsed;
                spKeys.Visibility = Visibility.Visible;
                btnSaveDumpKeys.Visibility = Visibility.Visible;
                btnAppendDumpKeys.Visibility = Visibility.Visible;

                dumpA.BinaryOutput = System.IO.File.ReadAllBytes(dumpA.FileName).ToList();

                if (converter.CheckDump(dumpA.FileName) == FileType.Text)
                {
                    var dr = System.Windows.MessageBox.Show(Translate.Key(nameof(MifareWindowsTool.Properties.Resources.thisismctdumpfile))
                          , "MCT Dump --> MWT Dump", MessageBoxButton.YesNoCancel, MessageBoxImage.Information, MessageBoxResult.Yes);
                    if (dr == MessageBoxResult.Yes)
                        dumpA = converter.ConvertToBinaryDump(dumpA);
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

        private void btnSaveDumpKeys_Click(object sender, RoutedEventArgs e)
        {
            if (!dumpA.Keys.Any())
            {
                MessageBox.Show("Nothing to save");
                return;
            }
            sfd.Filter = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.KeyFilesFilter));
            sfd.InitialDirectory = Tools.DefaultKeysPath;
            sfd.FileName = Path.GetFileNameWithoutExtension(dumpA.FileName);
            var dr = sfd.ShowDialog();
            if (dr.Value)
                System.IO.File.WriteAllText(sfd.FileName, string.Join(Environment.NewLine, dumpA.Keys.Distinct()));
        }

        private void btnAppendDumpKeys_Click(object sender, RoutedEventArgs e)
        {
            if (!dumpA.Keys.Any())
            {
                MessageBox.Show("Nothing to save");
                return;
            }
            ofd.Filter = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.KeyFilesFilter));
            ofd.InitialDirectory = Tools.DefaultKeysPath;

            ofd.FileName = Path.GetFileNameWithoutExtension(dumpA.FileName);
            var dr = ofd.ShowDialog();
            if (dr.Value)
                System.IO.File.AppendAllLines(ofd.FileName, dumpA.Keys.Distinct());

        }
    }
}
