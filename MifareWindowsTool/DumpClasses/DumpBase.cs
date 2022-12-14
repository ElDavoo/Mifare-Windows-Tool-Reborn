using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;

using Microsoft.Win32;

using MifareWindowsTool.DumpClasses;
using MifareWindowsTool.Properties;

using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using File = System.IO.File;

namespace MifareWindowsTool.Common
{
    public enum DumpType
    {
        NotSet, Mct, MWT, Flipper
    }
    public enum CardType
    {
        MifareMini, Mifare1K, Mifare2K, Mifare4K, NTag213, NTag215, NTag216, Unknown
    }
    public interface IDump
    {
        string DumpFileFullName { get; set; }
        string DumpFileNameWithoutExt { get; }
        string DumpFileName { get; }
        string StrDumpUID { get; set; }
        IData DumpData { get; set; }
        List<string> DumpKeys { get; set; }
        void ConvertFrom(IDump originDump, bool fillWithEmpty = true);
        void CompareTo(IDump dump, RichTextBox rtb);
        string ShowHexAndAddDumpKeys(RichTextBox txtOutput);
        void ShowAscii(RichTextBox txtOutput);
        int SectorCount { get; set; }
        int BlockSplit { get; set; }
        int BlockOrPageSize { get; set; }
        string StrDumpType { get; }
        string DefaultDumpExtension { get; set; }
        bool IsValid { get; }
        DumpType DumpType { get; }
        CardType CardType { get; }

    }
    public interface IData
    {
        bool IsValidForThisType { get; }
        byte[] HexData { get; set; }
        string TextData { get; set; }
        List<string> LstTextData { get; }
        List<byte[]> DataBlocks { get; }
        List<byte[]> DataLines { get; }
    }
    public class Data : IData
    {
        byte[][] BufferSplit(byte[] buffer, int blockSize)
        {
            if (buffer == null || buffer.Length == 0)
            {
                return new byte[0][];
            }
            byte[][] blocks = new byte[(buffer.Length + blockSize - 1) / blockSize][];

            for (int i = 0, j = 0; i < blocks.Length; i++, j += blockSize)
            {
                blocks[i] = new byte[Math.Min(blockSize, buffer.Length - j)];
                Array.Copy(buffer, j, blocks[i], 0, blocks[i].Length);
            }

            return blocks;
        }
        public byte[] HexData { get; set; }
        public List<byte[]> DataBlocks => BufferSplit(HexData, 16 * 4).ToList();
        public List<byte[]> DataLines => DataBlocks.SelectMany(d => BufferSplit(d, 16)).ToList();
        public string TextData { get; set; }
        public List<string> LstTextData => TextData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
        public virtual bool IsValidForThisType { get; }
    }

    public abstract class DumpBase : IDump
    {
        static byte[] DefaultBytesBLockToAppend = StringToByteArray("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000FFFFFFFFFFFFFF078069FFFFFFFFFFFF");
        public int BlockOrPageSize { get; set; } = 16;
        public static string CurrentUID { get; set; }
        public int BlockSplit { get; set; } = 8;

        Brush VioletBrush = new SolidColorBrush(Color.FromArgb(255, (byte)0x95, (byte)0x33, (byte)0xF9));
        public static string DefaultWorkingDir => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string DefaultNfcToolsPath { get; set; } = Path.Combine(DefaultWorkingDir, "nfctools");
        public static string DefaultDumpPath { get; set; } = Path.Combine(DefaultWorkingDir, "dumps");
        public static string DefaultKeysPath { get; set; } = Path.Combine(DefaultWorkingDir, "keys");
        public static string FlipperNfc1KPath => Path.Combine(DefaultNfcToolsPath, "Template_Flipper_1K.nfc");
        public static string FlipperNfcNTag213Path => Path.Combine(DefaultNfcToolsPath, "Template_Flipper_NTAG213.nfc");
        public static List<string> TemplateFlipperNfc1k => File.Exists(FlipperNfc1KPath) ? File.ReadAllText(FlipperNfc1KPath).Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList() : null;
        public static List<string> TemplateFlipperNfcNTag213 => File.Exists(FlipperNfcNTag213Path) ? File.ReadAllText(FlipperNfcNTag213Path).Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList() : null;

        public void CompareTo(IDump dump, RichTextBox rtb)
        {
            if (this.DumpData.HexData == null || dump.DumpData.HexData == null) return;

            rtb.Document = new System.Windows.Documents.FlowDocument();

            var sectorName = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.Sector));
            var identicalName = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.Identical));
            var differentName = Translate.Key(nameof(MifareWindowsTool.Properties.Resources.Different));

            var linesA = this.DumpData.DataLines;
            var linesB = dump.DumpData.DataLines;

            int idx = 0;
            var blockCpt = 0;
            foreach (var lineA in linesA)
            {
                if (idx % 4 == 0)
                {
                    rtb.AppendText($"{(blockCpt > 0 ? "\r" : "")}{sectorName}:{blockCpt}\r", Brushes.White, true);
                    blockCpt++;
                }
                byte[] lineB = null;
                if (idx < linesB.Count())
                {
                    lineB = linesB[idx];
                }
                if (ByteArrayCompare(lineA, lineB))
                {
                    rtb.AppendText("____________");
                    rtb.AppendText(identicalName, Brushes.Lime);
                    rtb.AppendText("_____________\r", Brushes.White);
                }
                else
                {
                    rtb.AppendText("____________");
                    rtb.AppendText(differentName, Brushes.Red);
                    rtb.AppendText("_____________\r", Brushes.White);

                    for (int l = 0; l < lineA.Count(); l++)
                    {
                        if (l == 0) //skip 2 chars for "A:" or "B:"
                        {
                            rtb.AppendText("  ");
                        }
                        if (lineB != null && l < lineB.Length)
                        {
                            if (lineA[l] != lineB[l])
                                rtb.AppendText("vv", Brushes.Red); //different
                            else
                                rtb.AppendText("  ", Brushes.White); //same
                        }
                    }
                    rtb.AppendText("\r", Brushes.White); //add cr at the end of line
                }
                var strLineA = BitConverter.ToString(lineA).Replace("-", string.Empty);
                var strLineB = lineB != null ? BitConverter.ToString(lineB).Replace("-", string.Empty) : string.Empty;
                var diff = strLineA.Length - strLineB.Length;
                var addToB = new string('-', diff);

                rtb.AppendText($"A:{strLineA}{Environment.NewLine}", Brushes.White);
                rtb.AppendText($"B:{strLineB}", Brushes.White);
                rtb.AppendText($"{addToB}{Environment.NewLine}", Brushes.Yellow);
                idx++;
            }

        }
        // byte[] is implicitly convertible to ReadOnlySpan<byte>
        bool ByteArrayCompare(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
        {
            return a2 != null && a1.SequenceEqual(a2);
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

        public int sectorCounter = 0;
        public int blockCountInSector = 4;
        public int SectorCount { get; set; } = 16;
        public void ShowAscii(RichTextBox txtOutput)
        {
            sectorCounter = 0;
            txtOutput.Document = new System.Windows.Documents.FlowDocument();
            byte[][] chunks = BufferSplit(this.DumpData.HexData.ToArray(), BlockOrPageSize);
            for (int i = 0; i < chunks.GetLength(0); i++)
            {
                if (SectorCount == 40 && i > 128)
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
        protected void SetCardType()
        {
            if (this.DumpData == null || this.DumpData.HexData == null) return;
            switch (this.DumpData.HexData.Length)
            {
                case 320: this.CardType = CardType.MifareMini; break;
                case 1024: this.CardType = CardType.Mifare1K; break;
                case 2048: this.CardType = CardType.Mifare2K; break;
                case 4096: this.CardType = CardType.Mifare4K; break;
                case 180: this.CardType = CardType.NTag213; break;
                case 540: this.CardType = CardType.NTag215; break;
                case 924: this.CardType = CardType.NTag216; break;
                default: break;
            }
        }
        public string ShowHexAndAddDumpKeys(RichTextBox txtOutput)
        {
            if (txtOutput == null) return null;
            this.DumpKeys.Clear();
            this.DumpKeys.Add($"# dump keys from {this.DumpFileName} added on {DateTime.Now.Date:MMMM dd yyyy}");
            sectorCounter = 0;
            txtOutput.Document = new System.Windows.Documents.FlowDocument();

            byte[][] chunks = BufferSplit(this.DumpData.HexData.ToArray(), BlockOrPageSize);

            for (int i = 0; i < chunks.GetLength(0); i++)
            {
                if (SectorCount == 40 && i > 128)
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
                    for (int c = 0; c < BlockOrPageSize * 2; c++)
                    {
                        if (strBlock.Length <= c) continue;
                        if (c <= 11)
                        {
                            txtOutput.AppendText(strBlock[c].ToString(), Brushes.Lime);

                        }
                        else if (c > 11 && c <= 19) txtOutput.AppendText(strBlock[c].ToString(), Brushes.Orange);
                        else
                        {
                            txtOutput.AppendText(strBlock[c].ToString(), Brushes.Green);
                        }
                    }
                    //add keys to dump
                    var keyA =strBlock.Length > 12 ? strBlock.Substring(0, 12) : string.Empty;
                    if (!string.IsNullOrEmpty(keyA) && !this.DumpKeys.Contains(keyA))
                        this.DumpKeys.Add(keyA);
                    var keyB = strBlock.Length >= 32 ? strBlock.Substring(20, 12) : string.Empty;
                    if (!string.IsNullOrEmpty(keyB) && !this.DumpKeys.Contains(keyB))
                        this.DumpKeys.Add(keyB);

                    txtOutput.AppendText(Environment.NewLine, Brushes.White);
                }
                else
                    txtOutput.AppendText(strBlock, Brushes.White);

            }
            return string.Join(Environment.NewLine, this.DumpKeys);

        }
        public static OpenFileDialog CreateOpenDialog(string title = null, string defaultExt = null, string filter = null, string initialDir = null, string fileName = null)
        {
            var dlg = new OpenFileDialog()
            {
                FilterIndex = 1,
                RestoreDirectory = true,
                CheckFileExists = true,
                CheckPathExists = true,
            };
            if (title == null) title = "Open";
            SetDlgParams(dlg, title, defaultExt, filter, initialDir, fileName);
            return dlg;
        }

        public static SaveFileDialog CreateSaveDialog(string title = null, string defaultExt = null, string filter = null, string initialDir = null, string fileName = null)
        {
            var dlg = new SaveFileDialog()
            {
                FilterIndex = 1,
                RestoreDirectory = true,
                CheckPathExists = true
            };
            if (title == null) title = "Save";
            SetDlgParams(dlg, title, defaultExt, filter, initialDir, fileName);
            return dlg;
        }
        private static void SetDlgParams(FileDialog dlg, string title, string defaultExt, string filter, string initialDir, string fileName)
        {
            if (defaultExt != null) dlg.DefaultExt = defaultExt;
            dlg.Title = title;
            dlg.Filter = filter ?? Translate.Key(nameof(MifareWindowsTool.Properties.Resources.DumpFileFilter));
            if (!string.IsNullOrEmpty(initialDir)) dlg.InitialDirectory = initialDir;
            if (fileName != null) dlg.FileName = fileName;
        }

        private const string FlipperFileIdentifier = "Filetype: Flipper NFC";
        private static List<string> FlipperAtqas = new List<string>() { "04 00", "44 00" };

        protected DumpBase()
        {

        }

        private static bool IsFlipperMifareClassic(string textData) =>

                 (IsValidTemplateFlipperNfc1k || IsValidTemplateFlipperNfcNtag213) &&
                      textData.IndexOf("ATQA: ") > 0 && textData.StartsWith(FlipperFileIdentifier)
                      && FlipperAtqas.Contains(textData.Substring(textData.IndexOf("ATQA: ") + "ATQA: ".Length, 5));


        public static bool IsValidTemplateFlipperNfc1k => TemplateFlipperNfc1k != null && TemplateFlipperNfc1k.Any() && !string.IsNullOrWhiteSpace(string.Join("", TemplateFlipperNfc1k)) && TemplateFlipperNfc1k[0].StartsWith(FlipperFileIdentifier);
        public static bool IsValidTemplateFlipperNfcNtag213 => TemplateFlipperNfcNTag213 != null && TemplateFlipperNfcNTag213.Any() && !string.IsNullOrWhiteSpace(string.Join("", TemplateFlipperNfcNTag213)) && TemplateFlipperNfcNTag213[0].StartsWith(FlipperFileIdentifier);
        private static IDump CreateTypedDump(Data data, string fileName)
        {
            if (data.TextData.StartsWith("+Sect"))// || (fi.Length != 1024 && fi.Length != 4096 && fi.Length != 320))
            {
                return new DumpMct(data, fileName);
            }
            else if (IsFlipperMifareClassic(data.TextData))
            {
                return new DumpFlipper(data, fileName);
            }
            else if (data.HexData != null)
            {
                return new DumpMWT(data, fileName);
            }
            return null;
        }
        protected abstract byte[] CreateBinaryDataFromText(IDump originDump);
        public void ConvertFrom(IDump originDump, bool fillWithEmptyDefault = true)
        {
            var tmpString = PrepareTextFromHex(originDump, fillWithEmptyDefault);
            LocalConvertFrom(tmpString);
        }
        protected abstract void LocalConvertFrom(string tmpString);
        private string PrepareTextFromHex(IDump originDump, bool fillWithEmptyDefault)
        {
            int lineSize, dumpSize;
            byte[] bytesData;
            SetDumpSizes(originDump, out lineSize, out bytesData, out dumpSize);
            var isNTag = originDump.CardType == CardType.NTag213 || originDump.CardType == CardType.NTag215 || originDump.CardType == CardType.NTag216;
            if (fillWithEmptyDefault && !isNTag)
            {
                //fill until block is complete
                while (bytesData.Length % BlockOrPageSize != 0)
                {
                    var partialFill = FillHexDataLine(lineSize, bytesData);
                    if (partialFill == null)
                    {
                        bytesData = FillHexDataLine(BlockOrPageSize, bytesData);
                        break;
                    }
                    bytesData = partialFill;
                }
                //fill until dump is complete
                while (bytesData.Length < dumpSize)
                {
                    bytesData = bytesData.Concat(DefaultBytesBLockToAppend).ToArray();
                }
            }
            var strUidLength = isNTag ? 16 : 8;
            string hex = BitConverter.ToString(bytesData).Replace("-", string.Empty);
            if (hex.Length >= strUidLength) this.StrDumpUID = hex.Substring(0, strUidLength);
            var splitLength = isNTag ? 8 : 32;
            return string.Join("\r\n", Split(hex, splitLength));
        }

        private void SetDumpSizes(IDump originDump, out int lineSize, out byte[] bytesData, out int dumpSize)
        {
            lineSize = 16;
            BlockOrPageSize = lineSize * 4;
            bytesData = originDump.DumpData.HexData;
            dumpSize = BlockOrPageSize * 16;
            switch (originDump.CardType)
            {
                case CardType.NTag213:
                    BlockOrPageSize = 4;
                    lineSize = 4;
                    dumpSize = 180;
                    break;
                case CardType.NTag215:
                    BlockOrPageSize = 4;
                    lineSize = 4;
                    dumpSize = 540;
                    break;
                case CardType.NTag216:
                    BlockOrPageSize = 4;
                    lineSize = 4;
                    dumpSize = 924;
                    break;

                case CardType.Unknown:
                    break;

            }

        }

        private byte[] FillHexDataLine(int dataSize, byte[] bytesData)
        {
            var mod = bytesData.Length % dataSize;
            if (mod != 0)
            {
                var diff = dataSize - mod;
                var defaultLength = DefaultBytesBLockToAppend.Length;
                var appendDefault = DefaultBytesBLockToAppend.Skip(defaultLength - diff);
                return bytesData.Concat(appendDefault).ToArray();
            }
            return null;
        }

        public string StrDumpUID { get; set; }
        public List<string> DumpKeys { get; set; } = new List<string>();
        public string DumpFileFullName { get; set; }
        public IData DumpData { get; set; } = new Data();
        public string DumpFileNameWithoutExt => Path.GetFileNameWithoutExtension(DumpFileFullName);
        public string DumpFileName => Path.GetFileName(DumpFileFullName);
        public string StrDumpType => this.GetType().Name.Replace("Dump", "");
        public virtual string DefaultDumpExtension { get; set; }
        public virtual bool IsValid => !string.IsNullOrWhiteSpace(DumpFileFullName) && System.IO.File.Exists(DumpFileFullName) && new System.IO.FileInfo(DumpFileFullName).Length > 0;
        public abstract DumpType DumpType { get; }

        public CardType CardType { get; internal set; } = CardType.Unknown;

        public static IDump OpenCreateDump(out bool canceled, string title = null)
        {
            canceled = false;
            var dlgTitle = title ?? Translate.Key(nameof(MifareWindowsTool.Properties.Resources.OpenDump));
            var dlg = CreateOpenDialog(dlgTitle);
            var dr = dlg.ShowDialog();
            if (!dr.HasValue || !dr.Value)
            {
                canceled = true;
                return null;
            }
            return OpenExistingDump(dlg.FileName);
        }
        public static IDump OpenExistingDump(string fullFileName)
        {
            var textData = System.IO.File.ReadAllText(fullFileName);
            if (string.IsNullOrWhiteSpace(textData)) return null;
            return CreateTypedDump(new Data() { TextData = textData, HexData = System.IO.File.ReadAllBytes(fullFileName) }, fullFileName);
        }
        protected static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length).Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
        }
        public List<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize).Select(i => str.Substring(i * chunkSize, chunkSize)).ToList();
        }

    }


}
