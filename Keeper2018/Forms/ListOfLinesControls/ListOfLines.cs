using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Keeper2018
{
    public class ListOfLines
    {
        private int _maxWidth;
        public List<ListLine> Lines { get; set; } = new List<ListLine>();

        public ListOfLines() { _maxWidth = 50; }

        public ListOfLines(int maxWidth) { _maxWidth = maxWidth; }

       
        public void Add(string str, FontWeight fontWeight, Brush foreground, int fontSize = 12)
        {
            var ss = TruncateLine(str);
            foreach (var line in ss)
                Lines.Add(new ListLine(line, fontWeight, foreground, fontSize, fontSize + 7));
        }

        public void Add(string str, FontWeight fontWeight, int fontSize = 12)
        {
            var ss = TruncateLine(str);
            foreach (var line in ss)
                Lines.Add(new ListLine(line, fontWeight, Brushes.Black, fontSize, fontSize + 7 ));
        }

        public void Add(string str, Brush foreground, int fontSize = 12)
        {
            var ss = TruncateLine(str);
            foreach (var line in ss)
                Lines.Add(new ListLine(line, FontWeights.Normal, foreground, fontSize, fontSize + 7 ));
        }

        public void Add(string str, int fontSize = 12)
        {
            var ss = TruncateLine(str);
            foreach (var line in ss)
                Lines.Add(new ListLine(line, FontWeights.Normal, Brushes.Black, fontSize, fontSize + 7));
        }

        public void AddRange(IEnumerable<ListLine> lines)
        {
            foreach (var line in lines)
            {
                Lines.Add(line);
            }
        }

        public void AddList(ListOfLines list)
        {
            foreach (var listLine in list.Lines) { Lines.Add(listLine); }
        }

        private List<string> TruncateLine(string str)
        {
            var result = new List<string>();
            while (str.Length > _maxWidth)
            {
                var str1 = str.Substring(0, 50);
                var pos = str1.LastIndexOf(' ');
                if (pos == -1) break;
                result.Add(str.Substring(0, pos));
                str = "       " + str.Substring(pos + 1);
            }
            result.Add(str);
            return result;
        }

    }
}