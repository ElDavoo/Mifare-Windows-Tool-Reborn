using System;
using System.Collections.Generic;
using System.Linq;

using MifareWindowsTool.Common;

namespace MifareWindowsTool.DumpClasses
{
    public interface IDumpMWT : IDump { }
    public class DumpMWT : DumpBase, IDumpMWT
    {
        public DumpMWT(Data data = null, string fileName = null)
        {
            DefaultDumpExtension = ".mfd";
            if (fileName != null) this.DumpFileFullName = fileName;
            if (data == null) return;
            this.DumpData = data;
            this.StrDumpUID = BitConverter.ToString(this.DumpData.HexData.Take(4).ToArray()).Replace("-", "");

        }
        public override bool IsValid => base.IsValid && DumpType == DumpType.MWT;
        public override DumpType DumpType => DumpType.MWT;
        protected override byte[] CreateBinaryDataFromText(IDump dmp)
        {
            List<byte> byteData = new List<byte>();
            foreach (var line in dmp.DumpData.LstTextData)
            {
                byteData.AddRange(StringToByteArray(line.Replace("-", "F")));
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
            this.DumpData.HexData = CreateBinaryDataFromText(this);
        }
    }
}
