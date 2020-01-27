using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MifareWindowsTool.Common
{
    public static class Extensions
    {
        public static void AppendText(this RichTextBox box, string text, Brush brush)
        {
            var textPointertext = box.Document.ContentEnd;
            TextPointer startPos = textPointertext.GetPositionAtOffset(0);
            TextPointer endPos = textPointertext.GetPositionAtOffset(text.Length);
            box.Selection.Select(startPos, endPos);
            box.AppendText(text);
            box.SelectionBrush = brush;
        }
    }
}
