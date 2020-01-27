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
        //public static void AppendText(this RichTextBox box, string text, Brush brush)
        //{
        //    BrushConverter bc = new BrushConverter();
        //    TextRange tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
        //    tr.Text = text;
        //    try
        //    {
        //        tr.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
        //    }
        //    catch (FormatException) { }
        //}

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
