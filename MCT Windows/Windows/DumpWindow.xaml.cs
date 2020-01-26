using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace MCT_Windows.Windows
{
    /// <summary>
    /// Logique d'interaction pour DumpWindow.xaml
    /// </summary>
    public partial class DumpWindow : Window
    {
        public List<string> Lines { get; set; } = new List<string>();
        byte[] bytesData = null;
        public DumpWindow(string fileName)
        {
            InitializeComponent();
            bytesData = File.ReadAllBytes(fileName);
            string hex = BitConverter.ToString(bytesData).Replace("-", string.Empty);
            
            Lines=Split(hex, 32);
            int sector = (Lines.Count - 4) / 4;
            for (int i = Lines.Count - 4; i >= 0; i -= 4)
                Lines.Insert(i, $"+Sector: {sector--}\n");

            txtOutput.Text = new string(Lines.SelectMany(c => c).ToArray());
        }
        static List<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize) + "\r\n").ToList();
        }

        private void btnSaveDump_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Dump Files|*.dump";
            var dr = sfd.ShowDialog();
            if (dr.Value)
                File.WriteAllBytes(sfd.FileName, bytesData);

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
