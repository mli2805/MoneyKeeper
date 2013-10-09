using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Keeper.Utils;
using Keeper.ViewModels;

namespace Keeper.Controls
{
  public partial class BarDiagramControl : UserControl
  {
    public void Connect(int connectionId, object target)
    {
      throw new NotImplementedException();
    }

    public static readonly DependencyProperty AllDiagramDataProperty =
      DependencyProperty.Register("AllDiagramData", typeof(List<DiagramPair>),
                                  typeof(BarDiagramControl), new FrameworkPropertyMetadata(new List<DiagramPair>()));

    public List<DiagramPair> AllDiagramData
    {
      get { return (List<DiagramPair>)GetValue(AllDiagramDataProperty); }
      set { SetValue(AllDiagramDataProperty, value); }
    }

    private readonly double _canvasWidth;  // Не важно сколько конкретно - они займут всю ячейку грида
    private readonly double _canvasHeight;
    private const double LeftMargin = 50;
    private const double RightMargin = 50;
    private const double TopMargin = 30;
    private const double BottomMargin = 30;

    public List<DiagramPair> CurrentDiagramData { get; set; }

    private DateTime _minDate, _maxDate;
    private double _minValue, _maxValue, _lowestScaleValue;
    private double _pointPerOneValue;
    private double _pointPerDate, _pointPerBar;
    private double _gap; // промежуток между столбиками диаграммы
    private double _shift; // от левой оси до первого столбика

    public DrawingGroup DrawingGroup = new DrawingGroup();
    public DrawingImage ImageSource { get; set; }

    public BarDiagramControl()
    {
      InitializeComponent();

      _canvasHeight = SystemParameters.FullPrimaryScreenHeight;
      _canvasWidth = SystemParameters.FullPrimaryScreenWidth;

      this.Loaded += BarDiagramControlOnLoaded;
    }

    void BarDiagramControlOnLoaded(object sender, RoutedEventArgs e)
    {
      CurrentDiagramData = new List<DiagramPair>(AllDiagramData);
      if (CurrentDiagramData.Count == 0) return;
      DrawCurrentDiagram();
      DiagramImage.Source = ImageSource;

      var window = Window.GetWindow(this);
      if (window != null) window.KeyDown += OnKeyDown; 
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

      var rectGeometry = new RectangleGeometry { Rect = new Rect(0, 0, _canvasWidth, _canvasHeight) };
      geometryDrawing.Geometry = rectGeometry;
      geometryDrawing.Brush = Brushes.LightYellow; // Кисть закраски
      // Добавляем готовый слой в контейнер отображения
      DrawingGroup.Children.Add(geometryDrawing);

      var geometryDrawing2 = new GeometryDrawing();

      var rectGeometry2 = new RectangleGeometry
      {
        Rect = new Rect(LeftMargin, TopMargin, _canvasWidth - LeftMargin - RightMargin,
                        _canvasHeight - TopMargin - BottomMargin)
      };
      geometryDrawing2.Geometry = rectGeometry2;
      geometryDrawing2.Brush = Brushes.White; // Кисть закраски
      // Добавляем готовый слой в контейнер отображения
      DrawingGroup.Children.Add(geometryDrawing2);
    }

    private void HorizontalAxes()
    {
      var geometryGroup = new GeometryGroup();
      var bottomAxis = new LineGeometry(new Point(LeftMargin, _canvasHeight - BottomMargin),
                                        new Point(_canvasWidth - RightMargin, _canvasHeight - BottomMargin));
      geometryGroup.Children.Add(bottomAxis);

      var topAxis = new LineGeometry(new Point(LeftMargin, TopMargin),
                                     new Point(_canvasWidth - RightMargin, TopMargin));
      geometryGroup.Children.Add(topAxis);

      var geometryDrawing = new GeometryDrawing { Geometry = geometryGroup, Pen = new Pen(Brushes.Black, 1) };
      DrawingGroup.Children.Add(geometryDrawing);
    }

    private void HorizontalAxisWithMarkers(Dock flag)
    {
      _shift = 4;
      _pointPerDate = (_canvasWidth - LeftMargin - RightMargin - _shift) / CurrentDiagramData.Count;

      const double minPointBetweenMarkedDivision = 50;
      var markedDash = (int)Math.Ceiling(minPointBetweenMarkedDivision / _pointPerDate);

      var geometryGroupDashesAndMarks = new GeometryGroup();
      var geometryGroupGridlines = new GeometryGroup();

      for (var i = 0; i < CurrentDiagramData.Count; i++)
      {
        var dashY = flag == Dock.Bottom ? _canvasHeight - BottomMargin : TopMargin;
        var dash = new LineGeometry(new Point(LeftMargin + _shift / 2 + _pointPerDate * (i + 0.5), dashY - 5),
                                    new Point(LeftMargin + _shift / 2 + _pointPerDate * (i + 0.5), dashY + 5));
        geometryGroupDashesAndMarks.Children.Add(dash);


        if (i % markedDash == 0)
        {
          var gridline = new LineGeometry(new Point(LeftMargin + _shift / 2 + _pointPerDate * (i + 0.5) - 1, TopMargin + 5),
                                          new Point(LeftMargin + _shift / 2 + _pointPerDate * (i + 0.5) - 1,
                                                    _canvasHeight - BottomMargin - 5));
          geometryGroupGridlines.Children.Add(gridline);

          var markY = flag == Dock.Bottom ? _canvasHeight : TopMargin;
          var mark = String.Format("{0:M/yyyy} ", CurrentDiagramData[i].CoorXdate);
          var formattedText = new FormattedText(mark, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                                new Typeface("Times New Roman"), 12, Brushes.Black);
          var geometry =
            formattedText.BuildGeometry(new Point(LeftMargin + _shift / 2 + _pointPerDate * (i + 0.5) - 18, markY - 20));
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
                                      new Point(LeftMargin, _canvasHeight - BottomMargin));
      geometryGroup.Children.Add(leftAxis);
      var rightAxis = new LineGeometry(new Point(_canvasWidth - RightMargin, TopMargin),
                                       new Point(_canvasWidth - RightMargin, _canvasHeight - BottomMargin));
      geometryGroup.Children.Add(rightAxis);

      var geometryDrawing = new GeometryDrawing { Geometry = geometryGroup, Pen = new Pen(Brushes.Black, 1) };
      DrawingGroup.Children.Add(geometryDrawing);
    }

    private void MarkersForVerticalAxes(Dock flag)
    {
      const int precision = 0;
      const double minPointBetweenDivision = 35;

      double values = (_maxValue - _minValue) * Math.Pow(10, precision);
      _pointPerOneValue = (_canvasHeight - TopMargin - BottomMargin) / values;

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
      double pointPerScaleStep = (_canvasHeight - TopMargin - BottomMargin) / divisions;
      _lowestScaleValue = fromDivision * accurateValuesPerDivision;
      _pointPerOneValue = (_canvasHeight - TopMargin - BottomMargin) / (divisions * accurateValuesPerDivision);

      var geometryGroupDashesAndMarks = new GeometryGroup();
      var geometryGroupGridlines = new GeometryGroup();
      for (var i = 0; i <= divisions; i++)
      {
        var dashX = flag == Dock.Left ? LeftMargin : _canvasWidth - LeftMargin;
        var dash = new LineGeometry(new Point(dashX - 5, _canvasHeight - BottomMargin - pointPerScaleStep * i),
                                    new Point(dashX + 5, _canvasHeight - BottomMargin - pointPerScaleStep * i));
        geometryGroupDashesAndMarks.Children.Add(dash);

        var gridline = new LineGeometry(new Point(LeftMargin + 5, _canvasHeight - BottomMargin - pointPerScaleStep * i),
                                        new Point(_canvasWidth - LeftMargin - 5,
                                                  _canvasHeight - BottomMargin - pointPerScaleStep * i));
        if (i + fromDivision == 0)
          geometryGroupDashesAndMarks.Children.Add(gridline);
        else if (i != 0 && i != divisions) geometryGroupGridlines.Children.Add(gridline);

        var markX = flag == Dock.Left ? LeftMargin - 40 : _canvasWidth - RightMargin + 15;
        var mark = String.Format("{0} ", (i + fromDivision) * accurateValuesPerDivision / Math.Pow(10, precision));
        var formattedText = new FormattedText(mark, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                              new Typeface("Times New Roman"), 12, Brushes.Black);
        var geometry =
          formattedText.BuildGeometry(new Point(markX, _canvasHeight - BottomMargin - pointPerScaleStep * i - 7));
        geometryGroupDashesAndMarks.Children.Add(geometry);
      }

      var geometryDrawingDashesAndMarks = new GeometryDrawing { Geometry = geometryGroupDashesAndMarks, Pen = new Pen(Brushes.Black, 1) };
      DrawingGroup.Children.Add(geometryDrawingDashesAndMarks);

      var geometryDrawingGridlines = new GeometryDrawing { Geometry = geometryGroupGridlines, Pen = new Pen(Brushes.LightGray, 1) };
      DrawingGroup.Children.Add(geometryDrawingGridlines);
    }

    private void Diagram()
    {
      var content = new List<string>();

      _gap = _pointPerDate / 3;
      _pointPerBar = _pointPerDate - _gap;

      var geometryGroupPositive = new GeometryGroup();
      var geometryGroupNegative = new GeometryGroup();
      for (int i = 0; i < CurrentDiagramData.Count; i++)
      {
        Rect rect;
        if (CurrentDiagramData[i].CoorYdouble >= 0)
          rect = new Rect(LeftMargin + _shift / 2 + _gap / 2 + i * (_pointPerBar + _gap),
                          _canvasHeight - BottomMargin -
                          (CurrentDiagramData[i].CoorYdouble - _lowestScaleValue) * _pointPerOneValue,
                          _pointPerBar,
                          CurrentDiagramData[i].CoorYdouble * _pointPerOneValue);
        else
          rect = new Rect(LeftMargin + _shift / 2 + _gap / 2 + i * (_pointPerBar + _gap),
                          _canvasHeight - BottomMargin - (0 - _lowestScaleValue) * _pointPerOneValue,
                          _pointPerBar,
                          -CurrentDiagramData[i].CoorYdouble * _pointPerOneValue);

        content.Add(String.Format("{0} столбец {1:0.000} - {2:0.000}", i, rect.Left, rect.Left + rect.Width));

        var rectangleGeometry = new RectangleGeometry(rect);
        if (CurrentDiagramData[i].CoorYdouble > 0)
          geometryGroupPositive.Children.Add(rectangleGeometry);
        else
          geometryGroupNegative.Children.Add(rectangleGeometry);

      }

      File.WriteAllLines(@"d:\chartbart.txt", content);

      var geometryDrawingPositive = new GeometryDrawing
      {
        Geometry = geometryGroupPositive,
        Pen = new Pen(Brushes.Blue, 1),
        Brush = Brushes.Blue
      };
      DrawingGroup.Children.Add(geometryDrawingPositive);
      var geometryDrawingNegative = new GeometryDrawing
      {
        Geometry = geometryGroupNegative,
        Pen = new Pen(Brushes.Red, 1),
        Brush = Brushes.Red
      };
      DrawingGroup.Children.Add(geometryDrawingNegative);
    }

    #endregion


    private bool ChangeDiagramData(ChangeDiagramDataMode mode, int horizontal, int vertical)
    {
      int shiftDateRange;
      switch (mode)
      {
        case ChangeDiagramDataMode.ZoomIn:
          if (CurrentDiagramData.Count < 4) return true;
          shiftDateRange = (_maxDate - _minDate).Days * horizontal / 100;
          if (shiftDateRange < 31)
          {
            _minDate = _minDate.AddMonths(1);
            _maxDate = _maxDate.AddMonths(-1);
          }
          else
          {
            _minDate = _minDate.AddDays(shiftDateRange);
            _maxDate = _maxDate.AddDays(-shiftDateRange);
          }
          break;
        case ChangeDiagramDataMode.ZoomOut:
          shiftDateRange = (_maxDate - _minDate).Days * horizontal / 100;
          if (shiftDateRange < 31) shiftDateRange = 31;
          _minDate = _minDate.AddDays(-shiftDateRange);
          _maxDate = _maxDate.AddDays(shiftDateRange);
          break;
        case ChangeDiagramDataMode.Move:
          var percent = (int)(horizontal * 100 / _canvasWidth);
          shiftDateRange = (_maxDate - _minDate).Days * percent / 100;
          _minDate = _minDate.AddDays(-shiftDateRange);
          _maxDate = _maxDate.AddDays(-shiftDateRange);
          break;
        case ChangeDiagramDataMode.ZoomInRect:

          break;
      }
      ExtractDataBetweenLimits();
      return true;
    }

    public void ExtractDataBetweenLimits()
    {
      CurrentDiagramData.Clear();
      foreach (var diagramPair in AllDiagramData)
      {
        if (diagramPair.CoorXdate >= _minDate && diagramPair.CoorXdate <= _maxDate)
        {
          CurrentDiagramData.Add(diagramPair);
        }
      }
    }


    #region преобразует точки к данным диаграммы и запускает перерисовку

    // привязан к типу диаграммы (в данном случае - столбцовая, время - значение)
    public enum WhichDiagramBar
    {
      OnTheLeftOfCursor,
      OnTheRightOfCursor
    }

    public int Point2Number(Point point, WhichDiagramBar flag)
    {
      double margin = LeftMargin + _shift / 2 + _gap / 2;
      double barWithGap = _pointPerBar + _gap;
      double d = point.X - margin;
      if (d < 0) return 0;
      var count = (int)Math.Floor(d / barWithGap);
      var rest = d - count * barWithGap;
      if (rest < _pointPerBar) return count;
      if (flag == WhichDiagramBar.OnTheLeftOfCursor) return count;
      return count + 1;
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
      var numberFrom = Point2Number(leftTop, WhichDiagramBar.OnTheRightOfCursor);
      var numberTo = Point2Number(rightBottom, WhichDiagramBar.OnTheLeftOfCursor);
      if (numberTo - numberFrom < 3) return;
      var nuevoCurrentDiagramData = new List<DiagramPair>();
      for (int i = numberFrom; i <= numberTo; i++)
      {
        nuevoCurrentDiagramData.Add(CurrentDiagramData[i]);
      }
      CurrentDiagramData = nuevoCurrentDiagramData;
      DrawCurrentDiagram();
    }

    public void ShowAllDiagram()
    {
      CurrentDiagramData = new List<DiagramPair>(AllDiagramData);
      DrawCurrentDiagram();
    }

    #endregion


    #region mouse events handlers
    private Point _mouseRightButtonDownPoint;
    private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      _mouseRightButtonDownPoint = e.GetPosition(this);
    }
    private void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
      var pt = e.GetPosition(this);
      if (pt != _mouseRightButtonDownPoint)
        MoveDiagramData(ChangeDiagramDataMode.Move, (int)(pt.X - _mouseRightButtonDownPoint.X),
                        (int)(pt.Y - _mouseRightButtonDownPoint.Y));
    }

    private Point _mouseLeftButtonDownPoint;
    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      _mouseLeftButtonDownPoint = e.GetPosition(this);
    }
    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      Point pt = e.GetPosition(this);
      if (pt == _mouseLeftButtonDownPoint) return;

      if (pt.X < _mouseLeftButtonDownPoint.X)
      {
        var temp = pt.X;
        pt.X = _mouseLeftButtonDownPoint.X;
        _mouseLeftButtonDownPoint.X = temp;
      }
      if (pt.Y < _mouseLeftButtonDownPoint.Y)
      {
        var temp = pt.Y;
        pt.Y = _mouseLeftButtonDownPoint.Y;
        _mouseLeftButtonDownPoint.Y = temp;
      }

      if (pt.X - _mouseLeftButtonDownPoint.X < 4 || pt.Y - _mouseLeftButtonDownPoint.Y < 4) ShowAllDiagram();
      else
        ZoomRectDiagram(_mouseLeftButtonDownPoint, pt);
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
      ZoomDiagram(e.Delta < 0 ? ChangeDiagramDataMode.ZoomIn : ChangeDiagramDataMode.ZoomOut, 10, 0);
    }

    #endregion

    #region popup menu
    private void GroupByMonthes(object sender, RoutedEventArgs e)
    {
      _maxDate = new DateTime(_maxDate.Year, 12, 31);
      ExtractDataBetweenLimits();
      DrawCurrentDiagram();
    }

    private void GroupByYears(object sender, RoutedEventArgs e)
    {
      DefineYearsLimits();
      ExtractDataBetweenLimitsWithYearsGrouping();
      DrawCurrentDiagram();
    }
    private void ExtractDataBetweenLimitsWithYearsGrouping()
    {
      CurrentDiagramData.Clear();
      var startYearDate = new DateTime(0);
      double yearValue = 0;
      foreach (var diagramPair in AllDiagramData)
      {
        if (diagramPair.CoorXdate < _minDate || diagramPair.CoorXdate > _maxDate) continue;

        if (startYearDate.Year != diagramPair.CoorXdate.Year)
        {
          if (startYearDate.Year != 1)
          {
            CurrentDiagramData.Add(new DiagramPair(startYearDate, yearValue));
          }

          startYearDate = diagramPair.CoorXdate;
          yearValue = diagramPair.CoorYdouble;
        }
        else
        {
          yearValue += diagramPair.CoorYdouble;
        }
      }
      CurrentDiagramData.Add(new DiagramPair(startYearDate, yearValue));
    }

    private void DefineYearsLimits()
    {
      const int bottomLimit = 2002;
      var topLimit = DateTime.Today.Year;

      var startYear = _minDate.Year;
      var endYear = _maxDate.Year;

      if (endYear - startYear < 2)
      {
        if (startYear > bottomLimit && endYear < topLimit)
        {
          startYear--;
          endYear++;
        }
        else
        {
          if (startYear == bottomLimit) endYear++;
          else startYear--;
        }
      }

      _minDate = new DateTime(startYear, 1, 1);
      _maxDate = new DateTime(endYear, 12, 31);
    }
    #endregion

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.A && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) ShowAllDiagram();
    }

  }
}
