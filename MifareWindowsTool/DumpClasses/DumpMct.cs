using System;
using System.Collections.Generic;
using System.Linq;

using MifareWindowsTool.Common;

namespace MifareWindowsTool.DumpClasses
{
    public interface IDumpMct : IDump { }
    public class DumpMct : DumpBase, IDumpMct
    {
        public DumpMct(Data data = null, string fileName = null)
        {
            DefaultDumpExtension = ".txt";
            if (fileName != null) this.DumpFileFullName = fileName;
            if (data == null) return;
            this.DumpData = data;
            this.DumpData.HexData = CreateBinaryDataFromText(this);

            SetCardType();

        }
        public override DumpType DumpType => DumpType.Mct;

        protected override byte[] CreateBinaryDataFromText(IDump dmp)
        {
            List<byte> byteData = new List<byte>();
            foreach (var line in dmp.DumpData.LstTextData.Where(l => !l.Contains("Sect")))
            {
                byteData.AddRange(StringToByteArray(line.Replace("-", "0")));
            }
            if (dmp.DumpData.LstTextData[0].Length >= 8)
            {
                var uid = dmp.DumpData.LstTextData[0].Substring(0, 8);
                dmp.StrDumpUID = uid;
            }
            return byteData.ToArray();
        }
        protected override void LocalConvertFrom(string tmpString)
        {
            this.DumpData.TextData = tmpString;
            //if (bInsertSector)
            //{
            var tmpLst = this.DumpData.LstTextData;
            int sector = (this.DumpData.LstTextData.Count - 4) / 4;
            for (int i = this.DumpData.LstTextData.Count - 4; i >= 0; i -= 4)
                tmpLst.Insert(i, $"+Sector: {sector--}");
            //}
            this.DumpData.TextData = string.Join("\n", tmpLst);
            this.DumpData.HexData = CreateBinaryDataFromText(this);
        }

    }
}
