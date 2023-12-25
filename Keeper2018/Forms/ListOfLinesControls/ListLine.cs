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

        public FontFamily FontFamily { get; set; } = new FontFamily("Lucida Sans Typewriter");
        // другие моноширинные
        // FontFamily = new FontFamily("Courier New");
        // FontFamily = new FontFamily("Consolas"); 
        // FontFamily = new FontFamily("Lucida Console"); 


        public ListLine() { }

        public ListLine(string line)
        {
            Line = line;
        }

        public ListLine(string line, Brush color, int fontSize = 12, int lineHeight = 16)
        {
            Line = line;
            Foreground = color;
            FontSize = fontSize;
            TextLineHeight = lineHeight;
        }

        public ListLine(string line, FontWeight weight, Brush color, int fontSize = 12, int lineHeight = 16)
        {
            Line = line;
            FontWeight = weight;
            Foreground = color;
            FontSize = fontSize;
            TextLineHeight = lineHeight;
        }

    }
}