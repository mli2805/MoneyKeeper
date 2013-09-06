using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Caliburn.Micro;
using Keeper.Utils;

namespace Keeper.ViewModels
{
  class MonthlyResultDiagramViewModel : Screen 
  {
    private const double CanvasWidth = 1200;  // Не важно сколько конкретно - они займут всю ячейку грида
    private const double CanvasHeight = 900;
    private const double LeftMargin = 50;
    private const double RightMargin = 50;
    private const double TopMargin = 30;
    private const double BottomMargin = 30;

    public List<DiagramPair> AllDiagramData { get; set; }
    public List<DiagramPair> CurrentDiagramData { get; set; }

    private DateTime _minDate, _maxDate;
    private double _minValue, _maxValue, _lowestScaleValue;
    private double _pointPerDay, _pointPerOneValue;
    private double _pointPerDate, _shift;

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

    public MonthlyResultDiagramViewModel(Dictionary<DateTime, decimal> monthlyResults)
    {
      AllDiagramData = (from pair in monthlyResults
                         select new DiagramPair(pair.Key, (double)pair.Value)).ToList();
      CurrentDiagramData = new List<DiagramPair>(AllDiagramData);
      DrawCurrentDiagram();
      DiagramDataCtors.AverageMonthlyResults(monthlyResults);
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

      var geometryDrawing = new GeometryDrawing { Geometry = geometryGroup, Pen = new Pen(Brushes.Black, 1) };
      DrawingGroup.Children.Add(geometryDrawing);
    }

    private void HorizontalAxisWithMarkers(Dock flag)
    {
      _pointPerDate = Math.Floor((CanvasWidth - LeftMargin - RightMargin) / CurrentDiagramData.Count);
      _shift = ((CanvasWidth - LeftMargin - RightMargin) - _pointPerDate * CurrentDiagramData.Count) / 2;

      const double minPointBetweenMarkedDivision = 50;
      var markedDash = (int)Math.Ceiling(minPointBetweenMarkedDivision/_pointPerDate);

      var geometryGroupDashesAndMarks = new GeometryGroup();
      var geometryGroupGridlines = new GeometryGroup();

      for (var i = 0; i < CurrentDiagramData.Count; i++)
      {
        var dashY = flag == Dock.Bottom ? CanvasHeight - BottomMargin : TopMargin;
        var dash = new LineGeometry(new Point(LeftMargin + _shift + _pointPerDate * (i + 0.5), dashY - 5),
                                      new Point(LeftMargin + _shift + _pointPerDate * (i + 0.5), dashY + 5));
        geometryGroupDashesAndMarks.Children.Add(dash);


        if (i % markedDash == 0)
        {
          var gridline = new LineGeometry(new Point(LeftMargin + _shift + _pointPerDate * (i + 0.5) -1, TopMargin + 5),
                           new Point(LeftMargin + _shift + _pointPerDate * (i + 0.5) -1, CanvasHeight - BottomMargin - 5));
          geometryGroupGridlines.Children.Add(gridline);

          var markY = flag == Dock.Bottom ? CanvasHeight : TopMargin;
          var mark = String.Format("{0:M/yyyy} ", CurrentDiagramData[i].CoorXdate);
          var formattedText = new FormattedText(mark, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                                new Typeface("Times New Roman"), 12, Brushes.Black);
          var geometry = formattedText.BuildGeometry(new Point(LeftMargin + _shift + _pointPerDate * (i + 0.5) - 18, markY - 20));
          geometryGroupDashesAndMarks.Children.Add(geometry);
        }
      }

      var geometryDrawingDashesAndMarks = new GeometryDrawing { Geometry = geometryGroupDashesAndMarks, Pen = new Pen(Brushes.Black, 1) };
      DrawingGroup.Children.Add(geometryDrawingDashesAndMarks);

      var geometryDrawingGridlines = new GeometryDrawing { Geometry = geometryGroupGridlines, Pen = new Pen(Brushes.LightGray, 1) };
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

      var geometryDrawing = new GeometryDrawing { Geometry = geometryGroup, Pen = new Pen(Brushes.Black, 1) };
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
        valuesPerDivision = Math.Ceiling(minPointBetweenDivision/_pointPerOneValue);
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
      double pointPerScaleStep = (CanvasHeight - TopMargin - BottomMargin) / divisions;
      _lowestScaleValue = fromDivision*accurateValuesPerDivision;
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
      var gap = Math.Round(_pointPerDate / 3);
      var pointPerBar = _pointPerDate - gap;

      var geometryGroup = new GeometryGroup();
      for (int i = 0; i < CurrentDiagramData.Count; i++)
      {
        Rect rect;
        if (CurrentDiagramData[i].CoorYdouble >= 0)
          rect = new Rect(LeftMargin + _shift + i*(pointPerBar+gap),
                          CanvasHeight - BottomMargin - (CurrentDiagramData[i].CoorYdouble - _lowestScaleValue)*_pointPerOneValue,
                          pointPerBar,
                          CurrentDiagramData[i].CoorYdouble*_pointPerOneValue);
        else rect = new Rect(LeftMargin + _shift + i*(pointPerBar+gap),
                          CanvasHeight - BottomMargin - (0 - _lowestScaleValue) * _pointPerOneValue,
                          pointPerBar,
                          - CurrentDiagramData[i].CoorYdouble * _pointPerOneValue);

        var rectangleGeometry = new RectangleGeometry(rect);
//        rectangleGeometry.
        geometryGroup.Children.Add(rectangleGeometry);
      }
      var geometryDrawing = new GeometryDrawing { Geometry = geometryGroup, Pen = new Pen(Brushes.LimeGreen, 1) };
      DrawingGroup.Children.Add(geometryDrawing);
    }

    #endregion

    private void ChangeDiagramData(ChangeDiagramDataMode mode)
    {
      var tt = new Stopwatch();
      tt.Start();

      int shiftDateRange;
      DateTime newMinDate = _minDate;
      DateTime newMaxDate = _maxDate;
      switch (mode)
      {
        case ChangeDiagramDataMode.Increase:
          shiftDateRange = (_maxDate - _minDate).Days / 10;
          newMinDate = _minDate.AddDays(shiftDateRange);
          newMaxDate = _maxDate.AddDays(-shiftDateRange);
          break;
        case ChangeDiagramDataMode.Decrease:
          shiftDateRange = (_maxDate - _minDate).Days / 10;
          newMinDate = _minDate.AddDays(-shiftDateRange);
          newMaxDate = _maxDate.AddDays(shiftDateRange);
          break;
        case ChangeDiagramDataMode.ShiftLeft:
          break;
      }

      CurrentDiagramData =
        AllDiagramData.Where(pair => pair.CoorXdate >= newMinDate && pair.CoorXdate <= newMaxDate).ToList();

      tt.Stop();
      Console.WriteLine(tt.Elapsed);
    }

    public void IncreaseDiagram()
    {
      ChangeDiagramData(ChangeDiagramDataMode.Increase);
      DrawCurrentDiagram();
    }

    public void DecreaseDiagram()
    {
      ChangeDiagramData(ChangeDiagramDataMode.Decrease);
      DrawCurrentDiagram();
    }

  }
}
