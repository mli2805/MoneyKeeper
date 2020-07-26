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

        private List<string> SplitLine(string str)
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

        public void Add(string str, FontWeight fontWeight, Brush foreground, int fontSize = 12)
        {
            var ss = SplitLine(str);
            foreach (var line in ss)
                Lines.Add(new ListLine(line) { FontWeight = fontWeight, Foreground = foreground, FontSize = fontSize, TextLineHeight = fontSize + 7 });
        }

        public void Add(string str, FontWeight fontWeight, int fontSize = 12)
        {
            var ss = SplitLine(str);
            foreach (var line in ss)
                Lines.Add(new ListLine(line) { FontWeight = fontWeight, Foreground = Brushes.Black, FontSize = fontSize, TextLineHeight = fontSize + 7 });
        }

        public void Add(string str, Brush foreground, int fontSize = 12)
        {
            var ss = SplitLine(str);
            foreach (var line in ss)
                Lines.Add(new ListLine(line) { FontWeight = FontWeights.Normal, Foreground = foreground, FontSize = fontSize, TextLineHeight = fontSize + 7 });
        }

        public void Add(string str, int fontSize = 12)
        {
            var ss = SplitLine(str);
            foreach (var line in ss)
                Lines.Add(new ListLine(line) { FontWeight = FontWeights.Normal, Foreground = Brushes.Black, FontSize = fontSize, TextLineHeight = fontSize + 7 });
        }

        public void AddRange(List<string> strs)
        {
            foreach (var str in strs) { Add(str); }
        }

        public void AddList(ListOfLines list)
        {
            foreach (var listLine in list.Lines) { Lines.Add(listLine); }
        }
    }
}