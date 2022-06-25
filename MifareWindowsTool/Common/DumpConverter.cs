using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace MifareWindowsTool.Common
{
    public class Dump
    {
        private string fileName;

        public Dump()
        {
        }

        public List<string> Lines { get; set; } = new List<string>();
        public int LinesCount => Lines.Count;
        public List<byte> BinaryOutput { get; set; } = new List<byte>();
        public byte[] BinArray => BinaryOutput.ToArray();
        public List<string> Keys { get; set; } = new List<string>();
        public string TextOutput { get; set; }
        public string FileName
        {
            get => fileName;
            set => fileName = value;
        }
    }
    public enum FileType
    {
        Text, Binary
    }
    public class DumpConverter
    {

        String FileText { get; set; }
        public FileType CheckDump(string filename)
        {
            FileText = File.ReadAllText(filename);
            FileInfo fi = new FileInfo(filename);
            if (FileText.ToString().StartsWith("+Sector:") || (fi.Length != 1024 && fi.Length != 4096 && fi.Length != 320))
                return FileType.Text;
            else
                return FileType.Binary;


        }
        public Dump ConvertToBinaryDump(Dump md)
        {
            try
            {
                var ret = new List<Byte>();
                md.Lines.Clear();
                md.BinaryOutput.Clear();
                md.Lines = FileText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();

                foreach (var line in md.Lines.Where(l => !l.StartsWith("+Sector")))
                {
                    md.BinaryOutput.AddRange(StringToByteArray(line));
                }
                return md;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"failed to convert : {ex}");
            }
            return md;
        }

        public Dump ConvertToTextDump(string filename, bool bInsertSector = true)
        {
            var bytesData = File.ReadAllBytes(filename);
            string hex = BitConverter.ToString(bytesData).Replace("-", string.Empty);
            var md = new Dump();

            md.Lines = Split(hex, 32);
            if (bInsertSector)
            {
                int sector = (md.Lines.Count - 4) / 4;
                for (int i = md.Lines.Count - 4; i >= 0; i -= 4)
                    md.Lines.Insert(i, $"+Sector: {sector--}\n");
            }

            md.TextOutput = new string(md.Lines.SelectMany(c => c).ToArray());


            return md;
        }

        public byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length).Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
        }
        static List<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize) + "\r\n").ToList();
        }
    }
}
