using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Caliburn.Micro;
using Keeper.DomainModel;

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

    public List<CurrencyRate> Data { get; set; }
    private DateTime _minDate, _maxDate;
    private double _minValue, _maxValue;
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

    public RatesDiagramViewModel(List<CurrencyRate> data)
    {
      Data = data;
      ProcessDiagramData();

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

    private void ProcessDiagramData()
    {
      InitDiagramData();
    }

    private void InitDiagramData()
    {
//      Data = LoadRatesFromTxt.LoadData().Where(r => r.Currency == CurrencyCodes.BYR).ToList();

      _minDate = Data[0].BankDay;
      _maxDate = Data.Last().BankDay;
      _minValue = Data.Min(r => r.Rate); if (_minValue > 0) _minValue = 0;
      _maxValue = Data.Max(r => r.Rate) * 1.03;
    }

    private void Diagram()
    {
      var geometryGroup = new GeometryGroup();
      for (int i = 0; i < Data.Count - 1; i++)
      {
        var line = new LineGeometry(new Point((Data[i].BankDay - _minDate).Days * _pointPerDay + LeftMargin,
                                              CanvasHeight - BottomMargin - Data[i].Rate * _pointPerOneValue),
                                    new Point((Data[i+1].BankDay - _minDate).Days * _pointPerDay + LeftMargin,
                                              CanvasHeight - BottomMargin - Data[i+1].Rate * _pointPerOneValue));
        geometryGroup.Children.Add(line);
      }
      var geometryDrawing = new GeometryDrawing();
      geometryDrawing.Geometry = geometryGroup;
      geometryDrawing.Pen = new Pen(Brushes.LimeGreen, 1);
      DrawingGroup.Children.Add(geometryDrawing);
    }

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

      var geometryDrawing = new GeometryDrawing();
      geometryDrawing.Geometry = geometryGroup;
      geometryDrawing.Pen = new Pen(Brushes.Black, 1);
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
        var mark = String.Format("{0:d/M/yyyy} ", Data[0].BankDay.AddDays(i * daysPerDivision));
        var formattedText = new FormattedText(mark, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                              new Typeface("Times New Roman"), 12, Brushes.Black);
        var geometry = formattedText.BuildGeometry(new Point(pointPerScaleStep * i + LeftMargin - 25, markY - 20));
        geometryGroupDashesAndMarks.Children.Add(geometry);
      }

      var geometryDrawingDashesAndMarks = new GeometryDrawing();
      geometryDrawingDashesAndMarks.Geometry = geometryGroupDashesAndMarks;
      geometryDrawingDashesAndMarks.Pen = new Pen(Brushes.Black, 1);
      DrawingGroup.Children.Add(geometryDrawingDashesAndMarks);

      var geometryDrawingGridlines = new GeometryDrawing();
      geometryDrawingGridlines.Geometry = geometryGroupGridlines;
      geometryDrawingGridlines.Pen = new Pen(Brushes.LightGray, 1);
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

      var geometryDrawing = new GeometryDrawing();
      geometryDrawing.Geometry = geometryGroup;
      geometryDrawing.Pen = new Pen(Brushes.Black, 1);
      DrawingGroup.Children.Add(geometryDrawing);
    }

    private void MarkersForVerticalAxes(Dock flag)
    {
      const int precision = 0;
      const double minPointBetweenDivision = 35;

      double values = (_maxValue - _minValue) * Math.Pow(10,precision);
      _pointPerOneValue = (CanvasHeight - TopMargin - BottomMargin) / values;
      double valuesPerDivision = Math.Ceiling(minPointBetweenDivision / _pointPerOneValue);
      double zeros = Math.Floor(Math.Log10(valuesPerDivision));
      double accurateValuesPerDivision = Math.Ceiling(valuesPerDivision / Math.Pow(10, zeros)) * Math.Pow(10, zeros);

      int steps = Convert.ToInt32(Math.Floor(values / accurateValuesPerDivision));
      double pointPerScaleStep = _pointPerOneValue * accurateValuesPerDivision;

      var geometryGroupDashesAndMarks = new GeometryGroup();
      var geometryGroupGridlines = new GeometryGroup();
      for (var i = 1; i <= steps; i++)
      {
        var dashX = flag == Dock.Left ? LeftMargin : CanvasWidth - LeftMargin;
        var dash = new LineGeometry(new Point(dashX - 5, CanvasHeight - BottomMargin - pointPerScaleStep * i),
                                    new Point(dashX + 5, CanvasHeight - BottomMargin - pointPerScaleStep * i));
        geometryGroupDashesAndMarks.Children.Add(dash);

        var gridline = new LineGeometry(new Point(LeftMargin + 5, CanvasHeight - BottomMargin - pointPerScaleStep*i),
                            new Point(CanvasWidth - LeftMargin - 5, CanvasHeight - BottomMargin - pointPerScaleStep*i));
        geometryGroupGridlines.Children.Add(gridline);

        var markX = flag == Dock.Left ? LeftMargin - 40 : CanvasWidth - RightMargin + 15;
        var mark = String.Format("{0} ", i * accurateValuesPerDivision / Math.Pow(10, precision));
        var formattedText = new FormattedText(mark, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                              new Typeface("Times New Roman"), 12, Brushes.Black);
        var geometry = formattedText.BuildGeometry(new Point(markX, CanvasHeight - BottomMargin - pointPerScaleStep * i - 7));
        geometryGroupDashesAndMarks.Children.Add(geometry);
      }

      var geometryDrawingDashesAndMarks = new GeometryDrawing();
      geometryDrawingDashesAndMarks.Geometry = geometryGroupDashesAndMarks;
      geometryDrawingDashesAndMarks.Pen = new Pen(Brushes.Black, 1);
      DrawingGroup.Children.Add(geometryDrawingDashesAndMarks);

      var geometryDrawingGridlines = new GeometryDrawing();
      geometryDrawingGridlines.Geometry = geometryGroupGridlines;
      geometryDrawingGridlines.Pen = new Pen(Brushes.LightGray, 1);
      DrawingGroup.Children.Add(geometryDrawingGridlines);
    }
  }
}
