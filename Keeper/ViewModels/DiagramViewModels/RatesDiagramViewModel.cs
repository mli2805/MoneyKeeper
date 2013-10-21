using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using Keeper.Utils;

namespace Keeper.ViewModels
{
  class RatesDiagramViewModel : Screen
  {
    private const double CanvasWidth = 800;  // Не важно сколько конкретно - они займут всю ячейку грида
    private const double CanvasHeight = 640;
    private const double LeftMargin = 50;
    private const double RightMargin = 50;
    private const double TopMargin = 30;
    private const double BottomMargin = 30;

    public List<DiagramPair> AllDiagramData { get; set; }
    public List<DiagramPair> CurrentDiagramData { get; set; }

    private DateTime _minDate, _maxDate;
    private double _minValue, _maxValue, _lowestScaleValue;
    private double _pointPerDay, _pointPerOneValue;

    public DrawingGroup DrawingGroup = new DrawingGroup();
    private DrawingImage _imageSource;
    public DrawingImage ImageSource
    {
      get { return _imageSource; }
      set
      {
        if (Equals(value, _imageSource)) return;
        _imageSource = value;
        NotifyOfPropertyChange(() => ImageSource);
      }
    }

    public RatesDiagramViewModel(Dictionary<DateTime, decimal> data)
    {
      AllDiagramData = (from pair in data
                        select new DiagramPair(pair.Key, (double)pair.Value)).ToList();
      CurrentDiagramData = new List<DiagramPair>(AllDiagramData);
      DrawCurrentDiagram();
    }

    public void DrawCurrentDiagram()
    {
      GetDiagramDataLimits();

      DiagramBackground();

      HorizontalAxes();
      HorizontalAxisWithMarkers(Dock.Top);
      HorizontalAxisWithMarkers(Dock.Bottom);

      VerticalAxes();
      MarkersForVerticalAxes(Dock.Left);
      MarkersForVerticalAxes(Dock.Right);

      Diagram();
      ImageSource = new DrawingImage(DrawingGroup);
    }

    private void GetDiagramDataLimits()
    {
      _minDate = CurrentDiagramData[0].CoorXdate;
      _maxDate = CurrentDiagramData.Last().CoorXdate;
      _minValue = CurrentDiagramData.Min(r => r.CoorYdouble); 
      _maxValue = CurrentDiagramData.Max(r => r.CoorYdouble);
    }

    #region Drawing implementation

    private void DiagramBackground()
    {
      var geometryDrawing = new GeometryDrawing();

      var rectGeometry = new RectangleGeometry { Rect = new Rect(0, 0, CanvasWidth, CanvasHeight) };
      geometryDrawing.Geometry = rectGeometry;
      geometryDrawing.Brush = Brushes.LightYellow;// Кисть закраски
      // Добавляем готовый слой в контейнер отображения
      DrawingGroup.Children.Add(geometryDrawing);

      var geometryDrawing2 = new GeometryDrawing();

      var rectGeometry2 = new RectangleGeometry
                            {
                              Rect = new Rect(LeftMargin, TopMargin, CanvasWidth - LeftMargin - RightMargin, 
                                           CanvasHeight - TopMargin - BottomMargin)
                            };
      geometryDrawing2.Geometry = rectGeometry2;
      geometryDrawing2.Brush = Brushes.White;// Кисть закраски
      // Добавляем готовый слой в контейнер отображения
      DrawingGroup.Children.Add(geometryDrawing2);
    }

    private void HorizontalAxes()
    {
      var geometryGroup = new GeometryGroup();
      var bottomAxis = new LineGeometry(new Point(LeftMargin, CanvasHeight - BottomMargin),
                                  new Point(CanvasWidth - RightMargin, CanvasHeight - BottomMargin));
      geometryGroup.Children.Add(bottomAxis);

      var topAxis = new LineGeometry(new Point(LeftMargin, TopMargin),
                                  new Point(CanvasWidth - RightMargin, TopMargin));
      geometryGroup.Children.Add(topAxis);

      var geometryDrawing = new GeometryDrawing {Geometry = geometryGroup, Pen = new Pen(Brushes.Black, 1)};
      DrawingGroup.Children.Add(geometryDrawing);
    }

    private void HorizontalAxisWithMarkers(Dock flag)
    {
      const double minPointBetweenDivision = 75;

      int days = (_maxDate - _minDate).Days;
      _pointPerDay = (CanvasWidth - LeftMargin - RightMargin) / days;
      double daysPerDivision = Math.Ceiling(minPointBetweenDivision / _pointPerDay);

      int steps = Convert.ToInt32(Math.Floor(days / daysPerDivision));
      double pointPerScaleStep = _pointPerDay * daysPerDivision;

      var geometryGroupDashesAndMarks = new GeometryGroup();
      var geometryGroupGridlines = new GeometryGroup();

      for (var i = 0; i <= steps; i++)
      {
        if (i != 0)
        {
          var dashY = flag == Dock.Bottom ? CanvasHeight - BottomMargin : TopMargin;
          var dash = new LineGeometry(new Point(pointPerScaleStep * i + LeftMargin, dashY - 5),
                                      new Point(pointPerScaleStep * i + LeftMargin, dashY + 5));
          geometryGroupDashesAndMarks.Children.Add(dash);
        }

        var gridline = new LineGeometry(new Point(pointPerScaleStep*i + LeftMargin, TopMargin + 5),
                         new Point(pointPerScaleStep * i + LeftMargin, CanvasHeight - BottomMargin - 5));
        geometryGroupGridlines.Children.Add(gridline);

        var markY = flag == Dock.Bottom ? CanvasHeight : TopMargin;
        var mark = String.Format("{0:d/M/yyyy} ", CurrentDiagramData[0].CoorXdate.AddDays(i * daysPerDivision));
        var formattedText = new FormattedText(mark, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                              new Typeface("Times New Roman"), 12, Brushes.Black);
        var geometry = formattedText.BuildGeometry(new Point(pointPerScaleStep * i + LeftMargin - 25, markY - 20));
        geometryGroupDashesAndMarks.Children.Add(geometry);
      }

      var geometryDrawingDashesAndMarks = new GeometryDrawing
                                            {Geometry = geometryGroupDashesAndMarks, Pen = new Pen(Brushes.Black, 1)};
      DrawingGroup.Children.Add(geometryDrawingDashesAndMarks);

      var geometryDrawingGridlines = new GeometryDrawing
                                       {Geometry = geometryGroupGridlines, Pen = new Pen(Brushes.LightGray, 1)};
      DrawingGroup.Children.Add(geometryDrawingGridlines);
    }

    private void VerticalAxes()
    {
      var geometryGroup = new GeometryGroup();
      var leftAxis = new LineGeometry(new Point(LeftMargin, TopMargin),
                                  new Point(LeftMargin, CanvasHeight - BottomMargin));
      geometryGroup.Children.Add(leftAxis);
      var rightAxis = new LineGeometry(new Point(CanvasWidth - RightMargin, TopMargin),
                              new Point(CanvasWidth - RightMargin, CanvasHeight - BottomMargin));
      geometryGroup.Children.Add(rightAxis);

      var geometryDrawing = new GeometryDrawing {Geometry = geometryGroup, Pen = new Pen(Brushes.Black, 1)};
      DrawingGroup.Children.Add(geometryDrawing);
    }

    private void MarkersForVerticalAxes(Dock flag)
    {
      const int precision = 0;
      const double minPointBetweenDivision = 35;

      double values = (_maxValue - _minValue) * Math.Pow(10, precision);
      _pointPerOneValue = (CanvasHeight - TopMargin - BottomMargin) / values;

      double valuesPerDivision;
      double zeros;
      if (minPointBetweenDivision > _pointPerOneValue)
      {
        valuesPerDivision = Math.Ceiling(minPointBetweenDivision / _pointPerOneValue);
        zeros = Math.Floor(Math.Log10(valuesPerDivision));
      }
      else
      {
        valuesPerDivision = minPointBetweenDivision / _pointPerOneValue;
        zeros = Math.Floor(Math.Log10(valuesPerDivision));
      }


      double accurateValuesPerDivision = Math.Ceiling(valuesPerDivision / Math.Pow(10, zeros)) * Math.Pow(10, zeros);

      int fromDivision = Convert.ToInt32(Math.Floor(_minValue / accurateValuesPerDivision));
      int divisions = Convert.ToInt32(Math.Ceiling(values / accurateValuesPerDivision));
      if ((fromDivision + divisions) * accurateValuesPerDivision < _maxValue) divisions++;
      double pointPerScaleStep = (CanvasHeight - TopMargin - BottomMargin) / divisions;
      _lowestScaleValue = fromDivision * accurateValuesPerDivision;
      _pointPerOneValue = (CanvasHeight - TopMargin - BottomMargin) / (divisions * accurateValuesPerDivision);

      var geometryGroupDashesAndMarks = new GeometryGroup();
      var geometryGroupGridlines = new GeometryGroup();
      for (var i = 0; i <= divisions; i++)
      {
        var dashX = flag == Dock.Left ? LeftMargin : CanvasWidth - LeftMargin;
        var dash = new LineGeometry(new Point(dashX - 5, CanvasHeight - BottomMargin - pointPerScaleStep * i),
                                    new Point(dashX + 5, CanvasHeight - BottomMargin - pointPerScaleStep * i));
        geometryGroupDashesAndMarks.Children.Add(dash);

        var gridline = new LineGeometry(new Point(LeftMargin + 5, CanvasHeight - BottomMargin - pointPerScaleStep * i),
                            new Point(CanvasWidth - LeftMargin - 5, CanvasHeight - BottomMargin - pointPerScaleStep * i));
        geometryGroupGridlines.Children.Add(gridline);

        var markX = flag == Dock.Left ? LeftMargin - 40 : CanvasWidth - RightMargin + 15;
        var mark = String.Format("{0} ", (i + fromDivision) * accurateValuesPerDivision / Math.Pow(10, precision));
        var formattedText = new FormattedText(mark, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                              new Typeface("Times New Roman"), 12, Brushes.Black);
        var geometry = formattedText.BuildGeometry(new Point(markX, CanvasHeight - BottomMargin - pointPerScaleStep * i - 7));
        geometryGroupDashesAndMarks.Children.Add(geometry);
      }

      var geometryDrawingDashesAndMarks = new GeometryDrawing { Geometry = geometryGroupDashesAndMarks, Pen = new Pen(Brushes.Black, 1) };
      DrawingGroup.Children.Add(geometryDrawingDashesAndMarks);

      var geometryDrawingGridlines = new GeometryDrawing { Geometry = geometryGroupGridlines, Pen = new Pen(Brushes.LightGray, 1) };
      DrawingGroup.Children.Add(geometryDrawingGridlines);
    }

    private void Diagram()
    {
      var geometryGroup = new GeometryGroup();
      for (int i = 0; i < CurrentDiagramData.Count - 1; i++)
      {
        var line = new LineGeometry(new Point((CurrentDiagramData[i].CoorXdate - _minDate).Days * _pointPerDay + LeftMargin,
                                              CanvasHeight - BottomMargin - (CurrentDiagramData[i].CoorYdouble - _lowestScaleValue) * _pointPerOneValue),
                                    new Point((CurrentDiagramData[i + 1].CoorXdate - _minDate).Days * _pointPerDay + LeftMargin,
                                              CanvasHeight - BottomMargin - (CurrentDiagramData[i + 1].CoorYdouble - _lowestScaleValue) * _pointPerOneValue));
        geometryGroup.Children.Add(line);
      }
      var geometryDrawing = new GeometryDrawing { Geometry = geometryGroup, Pen = new Pen(Brushes.LimeGreen, 1) };
      DrawingGroup.Children.Add(geometryDrawing);
    }

    #endregion

    private bool ChangeDiagramData(ChangeDiagramDataMode mode, int horizontal, int vertical)
    {
      var tt = new Stopwatch();
      tt.Start();

      int shiftDateRange;
      DateTime newMinDate = _minDate;
      DateTime newMaxDate = _maxDate;
      switch (mode)
      {
        case ChangeDiagramDataMode.ZoomIn:
          if (CurrentDiagramData.Count < 4) return false;
          if (CurrentDiagramData.Count < 16)
          {
            CurrentDiagramData.RemoveAt(CurrentDiagramData.Count-1);
            CurrentDiagramData.RemoveAt(0);
            return true;
          }

          shiftDateRange = (_maxDate - _minDate).Days * horizontal / 100;
          if (shiftDateRange < 31) shiftDateRange = 31;
          newMinDate = _minDate.AddDays(shiftDateRange);
          newMaxDate = _maxDate.AddDays(-shiftDateRange);
          break;
        case ChangeDiagramDataMode.ZoomOut:
          shiftDateRange = (_maxDate - _minDate).Days * horizontal / 100;
          if (shiftDateRange < 31) shiftDateRange = 31;
          newMinDate = _minDate.AddDays(-shiftDateRange);
          newMaxDate = _maxDate.AddDays(shiftDateRange);
          break;
        case ChangeDiagramDataMode.Move:
          var percent = (int)(horizontal * 100 / CanvasWidth);
          shiftDateRange = (_maxDate - _minDate).Days * percent / 100;
          newMinDate = _minDate.AddDays(-shiftDateRange);
          newMaxDate = _maxDate.AddDays(-shiftDateRange);
          break;
        case ChangeDiagramDataMode.ZoomInRect:

          break;
      }

      CurrentDiagramData.Clear();
      foreach (var diagramPair in AllDiagramData)
      {
        if (diagramPair.CoorXdate >= newMinDate && diagramPair.CoorXdate <= newMaxDate)
        {
          CurrentDiagramData.Add(diagramPair);
        }
      }

      tt.Stop();
      Console.WriteLine(tt.Elapsed);

      return true;
    }

    #region преобразует точки к данным диаграммы и запускает перерисовку
    // привязан к типу диаграммы (в данном случае - линия, соединяющая точки "время - значение")

    public int Point2Number(Point point)
    {
      return (int)((point.X - LeftMargin));
    }

    public void ZoomDiagram(ChangeDiagramDataMode param, int horizontal, int vertical)
    {
      if (ChangeDiagramData(param, horizontal, vertical)) DrawCurrentDiagram();
    }

    public void MoveDiagramData(ChangeDiagramDataMode param, int horizontal, int vertical)
    {
      if (ChangeDiagramData(param, horizontal, vertical)) DrawCurrentDiagram();
    }

    public void ZoomRectDiagram(Point leftTop, Point rightBottom)
    {
      var numberFrom = Point2Number(leftTop);
      var numberTo = Point2Number(rightBottom);
      if (numberTo - numberFrom < 3) return;
      var nuevoCurrentDiagramData = new List<DiagramPair>();
      for (int i = numberFrom; i < numberTo; i++)
      {
        nuevoCurrentDiagramData.Add(CurrentDiagramData[i]);
      }
      CurrentDiagramData = nuevoCurrentDiagramData;
      DrawCurrentDiagram();
    }

    public void ZoomAllDiagram()
    {
      CurrentDiagramData = new List<DiagramPair>(AllDiagramData);
      DrawCurrentDiagram();
    }

    #endregion

    #region mouse events handlers
    //  не привязан к типу диаграммы, передаются точки, в которых было нажатие-отпускание мыши
    //  разница в пикселах между этими точками, направления прокрутки колеса и т.п.

    private Point _mouseRightButtonDownPoint;
    public void MouseRightButtonDown(MouseEventArgs args, IInputElement elem)
    {
      _mouseRightButtonDownPoint = args.GetPosition(elem);
    }

    public void MouseRightButtonUp(MouseEventArgs args, IInputElement elem)
    {
      var pt = args.GetPosition(elem);
      if (pt != _mouseRightButtonDownPoint)
        MoveDiagramData(ChangeDiagramDataMode.Move, (int)(pt.X - _mouseRightButtonDownPoint.X),
                                                        (int)(pt.Y - _mouseRightButtonDownPoint.Y));
    }

    private Point _mouseLeftButtonDownPoint;
    public void MouseLeftButtonDown(MouseEventArgs args, IInputElement elem)
    {
      _mouseLeftButtonDownPoint = args.GetPosition(elem);
    }

    public void MouseLeftButtonUp(MouseEventArgs args, IInputElement elem)
    {
      Point pt = args.GetPosition(elem);
      if (pt == _mouseLeftButtonDownPoint) return;

      if (pt.X < _mouseLeftButtonDownPoint.X) { var temp = pt.X; pt.X = _mouseLeftButtonDownPoint.X; _mouseLeftButtonDownPoint.X = temp; }
      if (pt.Y < _mouseLeftButtonDownPoint.Y) { var temp = pt.Y; pt.Y = _mouseLeftButtonDownPoint.Y; _mouseLeftButtonDownPoint.Y = temp; }

      if (pt.X - _mouseLeftButtonDownPoint.X < 4 || pt.Y - _mouseLeftButtonDownPoint.Y < 4) ZoomAllDiagram();
      else
        ZoomRectDiagram(_mouseLeftButtonDownPoint, pt);
    }

    public void MouseDoubleClick()
    {
      Console.WriteLine("DoubleClick");
    }

    public void MouseWheel(MouseWheelEventArgs args)
    {
      ZoomDiagram(args.Delta < 0 ? ChangeDiagramDataMode.ZoomIn : ChangeDiagramDataMode.ZoomOut, 10, 0);
    }

    #endregion

  }
}
