using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;

using MifareWindowsTool.Common;

namespace MifareWindowsTool.DumpClasses
{
    public interface IDumpFlipper : IDump
    {
        string StrDumpSpacedUID { get; }
    }
    public class DumpFlipper : DumpBase, IDumpFlipper
    {
        public override DumpType DumpType => DumpType.Flipper;
        public string StrDumpSpacedUID => StrDumpUID.Aggregate("", (result, c) => result += ((!string.IsNullOrEmpty(result) && (result.Length + 1) % 3 == 0) ? " " : "") + c.ToString());

        public DumpFlipper(Data data = null, string fileName = null)
        {
            DefaultDumpExtension = ".nfc";
            if (fileName != null) this.DumpFileFullName = fileName;
            if (data == null) return;
            this.DumpData = data;

            this.DumpData.HexData = CreateBinaryDataFromText(this);
            SetCardType();
            SetUID(this);
        }

        private void SetUID(IDump dmp)
        {
            var strUidLength = 2 * 4 + 4 - 1;
            if (dmp.CardType == CardType.NTag213) strUidLength = 2 * 7 + 7 - 1;
            const string flipperUid = "UID: ";
            var uidIndex = dmp.DumpData.TextData.IndexOf(flipperUid) + flipperUid.Length;
            if (uidIndex > 0 && dmp.DumpData.TextData.Substring(uidIndex).Length >= strUidLength)
            {
                dmp.StrDumpUID = dmp.DumpData.TextData.Substring(uidIndex, strUidLength).Replace(" ", "");
            }
        }

        protected override byte[] CreateBinaryDataFromText(IDump dmp)
        {
            List<byte> byteData = new List<byte>();
            var loopString = "Block ";
            if (dmp.DumpData.LstTextData.Count(l => l.Contains("Page ")) >= 45)
            {
                loopString = "Page ";
            }
            foreach (var line in dmp.DumpData.LstTextData.Where(l => l.StartsWith(loopString)))
            {
                var colonCharIndex = line.IndexOf(':');
                if (colonCharIndex <= 0) continue;
                var lineDataTrim = line.Substring(colonCharIndex + 1);
                byteData.AddRange(StringToByteArray(lineDataTrim.Replace(" ", "")));
            }

            return byteData.ToArray();
        }

        protected override void LocalConvertFrom(string tmpString)
        {
            this.DumpData.TextData = tmpString;

            var ft = TemplateFlipperNfc1k;
            var flipperBlockStartRow = 12;
            ft[5] = ft[5].Replace("UID:", $"UID: {this.StrDumpSpacedUID}");
            bool is4K = this.DumpData.LstTextData.Count == 256;
            if (is4K && !string.IsNullOrEmpty(ft[9]) && ft[9].ToUpper().Contains("MIFARE CLASSIC TYPE:"))
            {
                ft[9] = "Mifare Classic type: 4K";
            }
            for (int i = 0; i < this.DumpData.LstTextData.Count; i++)
            {
                var spacedData = this.DumpData.LstTextData[i].Aggregate("", (result, c) => result += ((!string.IsNullOrEmpty(result) && (result.Length + 1) % 3 == 0) ? " " : "") + c.ToString());
                if ((ft.Count - 1) < (flipperBlockStartRow + i))
                {
                    ft.Add($"Block {(flipperBlockStartRow + i):00}: {spacedData}");
                }
                else
                    ft[flipperBlockStartRow + i] = ft[flipperBlockStartRow + i].Replace(":", $": {spacedData}");
            }

            this.DumpData.TextData = String.Join("\n", ft) + "\n";
            this.DumpData.HexData = CreateBinaryDataFromText(this);
        }

        //public DumpBase ConvertFromBinaryToTextDump(DumpType outputTextFileType, string filename, bool bInsertSector = true)
        //{
        //    try
        //    {
        //        DumpBase md = PrepareDumpFromBinaryToTextData(filename);
        //        if (outputTextFileType == DumpType.Mct)
        //        {
        //            if (bInsertSector)
        //            {
        //                int sector = (md.TextDataLines.Count - 4) / 4;
        //                for (int i = md.TextDataLines.Count - 4; i >= 0; i -= 4)
        //                    md.TextDataLines.Insert(i, $"+Sector: {sector--}\n");
        //            }
        //            md.TextData = new string(md.TextDataLines.SelectMany(c => c).ToArray());

        //        }
        //        else if (outputTextFileType == DumpType.Flipper)
        //        {
        //            var ft = Tools.TemplateFlipperNfc;
        //            var flipperBlockStartRow = 12;
        //            ft[5].Replace("UID:", $"UID: {md.StrDumpSpacedUID}");
        //            for (int i = 0; i < md.TextDataLines.Count; i++)
        //                ft[flipperBlockStartRow + i].Replace(":", $": {md.TextDataLines[i]}");

        //            md.TextOutput = new string(ft.SelectMany(c => c).ToArray());
        //        }
        //        return md;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;// MessageBox.Show($"failed to convert MWT to MCT: {ex}");
        //    }
        //}
    }
}
