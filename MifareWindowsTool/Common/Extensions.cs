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

        public static void AppendText(this RichTextBox richTextBox, string text, Brush brush, bool bold = false)
        {

            richTextBox.AppendText(text);
            int nlcount = text.ToCharArray().Count(a => a == '\n');
            int len = text.Length + 3 * (nlcount) + 2; //newlines are longer, this formula works fine
            TextPointer myTextPointer1 = richTextBox.Document.ContentEnd.GetPositionAtOffset(-len);
            TextPointer myTextPointer2 = richTextBox.Document.ContentEnd.GetPositionAtOffset(-1);

            richTextBox.Selection.Select(myTextPointer1, myTextPointer2);

            richTextBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
            if (bold)
                richTextBox.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            else
                richTextBox.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);

        }
    }
}
