using System.Collections.Generic;
using System.Windows.Media;
using OxyPlot;

namespace Keeper.Utils.DiagramDomainModel
{
	public class DiagramSeries
	{
		public string Name;
		public Brush PositiveBrushColor;
		public Brush NegativeBrushColor;
	    public OxyColor OxyColor;
		public int Index;
		public List<DiagramPoint> Points;
	}
}