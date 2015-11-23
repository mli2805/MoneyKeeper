using System.Collections.Generic;
using System.Windows.Media;

namespace Keeper.Utils.DiagramDomainModel
{
	public class DiagramSeries
	{
		public string Name;
		public Brush PositiveBrushColor;
		public Brush NegativeBrushColor;
	    public OxyPlot.OxyColor OxyColor;
		public int Index;
		public List<DiagramPoint> Points;
	}
}