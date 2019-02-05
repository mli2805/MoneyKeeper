using System.Windows;
using System.Windows.Media;

namespace Keeper2018
{
    public class ListLine
    {
        public string Line { get; set; }
        public FontWeight FontWeight { get; set; } = FontWeights.Normal;
        public int FontSize { get; set; } = 12;
        public int TextLineHeight { get; set; } = 16;
        public Brush Foreground { get; set; } = Brushes.Black;

        public ListLine(){}

        public ListLine(string line){ Line = line; }
    }
}