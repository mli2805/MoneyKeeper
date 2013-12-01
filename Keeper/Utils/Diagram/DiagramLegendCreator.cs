using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Keeper.Utils.Diagram
{
  class DiagramLegendCreator
  {
    private readonly DiagramData _diagramData;

    public DiagramLegendCreator(DiagramData diagramData)
    {
      _diagramData = diagramData;
    }

    public DrawingImage Create()
    {
      var drawingGroup = new DrawingGroup();

//      drawingGroup.Children.Add(new GeometryDrawing
//                                  {
//                                    Geometry = new RectangleGeometry { Rect = new Rect(0, 0, 150, 500) }, 
//                                    Brush = Brushes.White
//                                  });
      var i = 0;
      foreach (var series in _diagramData.Data)
      {
        var geometryGroupColorIndicator = new GeometryGroup();
        var line = new LineGeometry(new Point(5, i * 20 + 28), new Point(35, i * 20 + 28));
        geometryGroupColorIndicator.Children.Add(line);
        drawingGroup.Children.Add(new GeometryDrawing{Geometry = geometryGroupColorIndicator, Pen = new Pen(series.PositiveBrushColor,3)});

        var geometryGroupSeriesNames = new GeometryGroup();
        var formattedText = new FormattedText(series.Name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                                new Typeface("New Curier"), 14, series.PositiveBrushColor);
        var geometry = formattedText.BuildGeometry(new Point(45, i*20 + 20));
        geometryGroupSeriesNames.Children.Add(geometry);

        drawingGroup.Children.Add(new GeometryDrawing { Geometry = geometryGroupSeriesNames, Pen = new Pen(Brushes.Black , 1) });

        i++;
      }

      return new DrawingImage(drawingGroup);
    }
  }
}
