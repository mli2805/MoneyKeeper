using System.Windows;
using System.Windows.Media;

namespace Keeper2018
{
    public class MyMessageBoxLineModel
    {
        public string Line { get; set; }
        public FontWeight FontWeight { get; set; } = FontWeights.Normal;
        public Brush Foreground { get; set; } = Brushes.Black;
    }
}