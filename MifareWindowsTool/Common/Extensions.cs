using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

using static System.Net.Mime.MediaTypeNames;

namespace MifareWindowsTool.Common
{
    public static class Extensions
    {
        public static bool OnlyHex(this string str)
        {
          return str.ToUpper().All("0123456789ABCDEF".Contains);
        }
        public static string HexStrToAscii(this string hexString)
        {
            try
            {
                string ascii = string.Empty;

                for (int i = 0; i < hexString.Length; i += 2)
                {
                    String hs = string.Empty;

                    hs = hexString.Substring(i, 2);
                    uint decval = System.Convert.ToUInt32(hs, 16);
                    char character = System.Convert.ToChar(decval);
                    if (char.IsLetterOrDigit(character))
                        ascii += character;
                    else
                        ascii += ".";

                }

                return ascii;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return string.Empty;
        }

        public static void AppendText(this RichTextBox box, string text, Brush brush, bool bold = false)
        {
            TextRange tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
            tr.Text = text;
            try
            {
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
                if (bold)
                    tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                else
                    tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            }
            catch (FormatException) { }
        }
    }
}
