using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MifareWindowsTool.Common
{
    public static class Extensions
    {
        public static bool OnlyHex(this string str)
        {
            // For C-style hex notation (0xFF) you can use @"\A\b(0[xX])?[0-9a-fA-F]+\b\Z"
            return System.Text.RegularExpressions.Regex.IsMatch(str, @"\A\b[0-9a-fA-F]+\b\Z");
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
