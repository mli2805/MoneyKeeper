using System.Collections.Generic;
using System.Windows.Media;

namespace Keeper.Utils.Diagram
{
	public class DiagramSeries
	{
		public string Name;
		public Brush PositiveBrushColor;
		public Brush NegativeBrushColor;
		public int Index;
		public List<DiagramPair> Data;
	}
}