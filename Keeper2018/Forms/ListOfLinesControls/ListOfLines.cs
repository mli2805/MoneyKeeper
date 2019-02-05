using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Keeper2018
{
    public class ListOfLines
    {
        public List<ListLine> Lines { get; set; } = new List<ListLine>();

        public void Add(string str, FontWeight fontWeight, Brush foreground, int fontSize = 12)
        {
            Lines.Add(new ListLine(str) { FontWeight = fontWeight, Foreground = foreground, FontSize = fontSize, TextLineHeight = fontSize + 7});
        }
        public void Add(string str, FontWeight fontWeight, int fontSize = 12)
        {
            Lines.Add(new ListLine(str) { FontWeight = fontWeight, Foreground = Brushes.Black, FontSize = fontSize, TextLineHeight = fontSize + 7 });
        }

        public void Add(string str, Brush foreground, int fontSize = 12)
        {
            Lines.Add(new ListLine(str) { FontWeight = FontWeights.Normal, Foreground = foreground, FontSize = fontSize, TextLineHeight = fontSize + 7 });
        }

        public void Add(string str, int fontSize = 12)
        {
            Lines.Add(new ListLine(str) { FontWeight = FontWeights.Normal, Foreground = Brushes.Black, FontSize = fontSize, TextLineHeight = fontSize + 7 });
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