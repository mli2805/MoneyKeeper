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
    public List<DiagramPair> CurrentDiagramData { get; set; }
    private DateTime _minDate, _maxDate;
    private double _minValue, _maxValue;
    private double LowestScaleValue {get {return _fromDivision * _accurateValuesPerDivision;}}

    public double ImageWidth { get { return IsLoaded ? ActualWidth : SystemParameters.FullPrimaryScreenWidth; } }
    public double ImageHeight { get { return IsLoaded ? (ActualHeight - StatusBar.ActualHeight) : SystemParameters.FullPrimaryScreenHeight; } }

    private double LeftMargin { get { return ImageWidth*0.03; }}
    private double RightMargin { get { return ImageWidth * 0.03; }}
    private double TopMargin { get { return ImageHeight*0.03; }}
    private double BottomMargin { get { return ImageHeight * 0.03; }}

    private double PointPerDate { get
    {
      if (CurrentDiagramData == null) return 0;
      if (CurrentDiagramData.Count == 0) return 0;
      return (ImageWidth - LeftMargin - RightMargin - Shift) / CurrentDiagramData.Count;
    } }
    private double Gap { get { return PointPerDate/3; } } // промежуток между столбиками диаграммы
    private double PointPerBar { get { return PointPerDate - Gap; } }
    private double Shift { get { return ImageWidth * 0.002; } } // от левой оси до первого столбика

//---------- vertical axes
    private double _accurateValuesPerDivision;
    private int _fromDivision;
    private int _divisions;
    private double PointPerScaleStep { get { return  (ImageHeight - TopMargin - BottomMargin) / _divisions;}}

    private double PointPerOneValueAfter { get { return (ImageHeight - TopMargin - BottomMargin) / (_divisions * _accurateValuesPerDivision); } }
    private double Y0
    {
      get
      {
        var temp = ImageHeight - BottomMargin;
        if (_fromDivision < 0) temp += PointPerScaleStep * _fromDivision;
        return temp;
      }
    }

    public DrawingGroup DrawingGroup = new DrawingGroup();
    public DrawingImage ImageSource { get; set; }

    public BarDiagramControl()
    {
      InitializeComponent();

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

    private void GetDiagramDataLimits()
    {
      _minDate = CurrentDiagramData[0].CoorXdate;
      _maxDate = CurrentDiagramData.Last().CoorXdate;
      _minValue = CurrentDiagramData.Min(r => r.CoorYdouble);
      _maxValue = CurrentDiagramData.Max(r => r.CoorYdouble);
    }

    #region Drawing implementation

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

    private void DiagramBackground()
    {
      var geometryDrawing = new GeometryDrawing();

      var rectGeometry = new RectangleGeometry { Rect = new Rect(0, 0, ImageWidth, ImageHeight) };
      geometryDrawing.Geometry = rectGeometry;
      geometryDrawing.Brush = Brushes.LightYellow; // Кисть закраски
      // Добавляем готовый слой в контейнер отображения
      DrawingGroup.Children.Add(geometryDrawing);

      var geometryDrawing2 = new GeometryDrawing();

      var rectGeometry2 = new RectangleGeometry
      {
        Rect = new Rect(LeftMargin, TopMargin, ImageWidth - LeftMargin - RightMargin,
                        ImageHeight - TopMargin - BottomMargin)
      };
      geometryDrawing2.Geometry = rectGeometry2;
      geometryDrawing2.Brush = Brushes.White; // Кисть закраски
      // Добавляем готовый слой в контейнер отображения
      DrawingGroup.Children.Add(geometryDrawing2);
    }

    private void HorizontalAxes()
    {
      var geometryGroup = new GeometryGroup();
      var bottomAxis = new LineGeometry(new Point(LeftMargin, ImageHeight - BottomMargin),
                                        new Point(ImageWidth - RightMargin, ImageHeight - BottomMargin));
      geometryGroup.Children.Add(bottomAxis);

      var topAxis = new LineGeometry(new Point(LeftMargin, TopMargin),
                                     new Point(ImageWidth - RightMargin, TopMargin));
      geometryGroup.Children.Add(topAxis);

      var geometryDrawing = new GeometryDrawing { Geometry = geometryGroup, Pen = new Pen(Brushes.Black, 1) };
      DrawingGroup.Children.Add(geometryDrawing);
    }

    private void HorizontalAxisWithMarkers(Dock flag)
    {

      const double minPointBetweenMarkedDivision = 50;
      var markedDash = (int)Math.Ceiling(minPointBetweenMarkedDivision / PointPerDate);

      var geometryGroupDashesAndMarks = new GeometryGroup();
      var geometryGroupGridlines = new GeometryGroup();

      for (var i = 0; i < CurrentDiagramData.Count; i++)
      {
        var dashY = flag == Dock.Bottom ? ImageHeight - BottomMargin : TopMargin;
        var dash = new LineGeometry(new Point(LeftMargin + Shift / 2 + PointPerDate * (i + 0.5), dashY - 5),
                                    new Point(LeftMargin + Shift / 2 + PointPerDate * (i + 0.5), dashY + 5));
        geometryGroupDashesAndMarks.Children.Add(dash);


        if (i % markedDash == 0)
        {
          var gridline = new LineGeometry(new Point(LeftMargin + Shift / 2 + PointPerDate * (i + 0.5) - 1, TopMargin + 5),
                                          new Point(LeftMargin + Shift / 2 + PointPerDate * (i + 0.5) - 1,
                                                    ImageHeight - BottomMargin - 5));
          geometryGroupGridlines.Children.Add(gridline);

          var markY = flag == Dock.Bottom ? ImageHeight : TopMargin;
          var mark = String.Format("{0:M/yyyy} ", CurrentDiagramData[i].CoorXdate);
          var formattedText = new FormattedText(mark, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                                new Typeface("Times New Roman"), 12, Brushes.Black);
          var geometry =
            formattedText.BuildGeometry(new Point(LeftMargin + Shift / 2 + PointPerDate * (i + 0.5) - 18, markY - 20));
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
                                      new Point(LeftMargin, ImageHeight - BottomMargin));
      geometryGroup.Children.Add(leftAxis);
      var rightAxis = new LineGeometry(new Point(ImageWidth - RightMargin, TopMargin),
                                       new Point(ImageWidth - RightMargin, ImageHeight - BottomMargin));
      geometryGroup.Children.Add(rightAxis);

      var geometryDrawing = new GeometryDrawing { Geometry = geometryGroup, Pen = new Pen(Brushes.Black, 1) };
      DrawingGroup.Children.Add(geometryDrawing);
    }

    private void MarkersForVerticalAxes(Dock flag)
    {
      const double minPointBetweenDivision = 35;

      double pointPerOneValueBefore = _maxValue.Equals(_minValue) ? 0 : (ImageHeight - TopMargin - BottomMargin) / (_maxValue - _minValue); 
      double valuesPerDivision = (minPointBetweenDivision > pointPerOneValueBefore) ?
                  Math.Ceiling(minPointBetweenDivision / pointPerOneValueBefore) : minPointBetweenDivision / pointPerOneValueBefore; 
      double zeros = Math.Floor(Math.Log10(valuesPerDivision));
      _accurateValuesPerDivision = Math.Ceiling(valuesPerDivision/Math.Pow(10, zeros))*Math.Pow(10, zeros);
      _fromDivision = Convert.ToInt32(Math.Floor(_minValue / _accurateValuesPerDivision));

        var temp = Convert.ToInt32(Math.Ceiling((_maxValue - _minValue) / _accurateValuesPerDivision));
        if ((_fromDivision + temp) * _accurateValuesPerDivision < _maxValue) temp++;
      _divisions = temp;

      var geometryGroupDashesAndMarks = new GeometryGroup();
      var geometryGroupGridlines = new GeometryGroup();
      for (var i = 0; i <= _divisions; i++)
      {
        var dashX = flag == Dock.Left ? LeftMargin : ImageWidth - LeftMargin;
        var dash = new LineGeometry(new Point(dashX - 5, ImageHeight - BottomMargin - PointPerScaleStep * i),
                                    new Point(dashX + 5, ImageHeight - BottomMargin - PointPerScaleStep * i));
        geometryGroupDashesAndMarks.Children.Add(dash);

        var gridline = new LineGeometry(new Point(LeftMargin + 5, ImageHeight - BottomMargin - PointPerScaleStep * i),
                                        new Point(ImageWidth - LeftMargin - 5,
                                                  ImageHeight - BottomMargin - PointPerScaleStep * i));
        if (i + _fromDivision == 0)
          geometryGroupDashesAndMarks.Children.Add(gridline);
        else if (i != 0 && i != _divisions) geometryGroupGridlines.Children.Add(gridline);

        var markX = flag == Dock.Left ? LeftMargin - 40 : ImageWidth - RightMargin + 15;
        var mark = String.Format("{0} ", (i + _fromDivision) * _accurateValuesPerDivision);
        var formattedText = new FormattedText(mark, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                              new Typeface("Times New Roman"), 12, Brushes.Black);
        var geometry =
          formattedText.BuildGeometry(new Point(markX, ImageHeight - BottomMargin - PointPerScaleStep * i - 7));
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

      var geometryGroupPositive = new GeometryGroup();
      var geometryGroupNegative = new GeometryGroup();
      for (int i = 0; i < CurrentDiagramData.Count; i++)
      {
        Rect rect;
        if (CurrentDiagramData[i].CoorYdouble >= 0)
          rect = new Rect(LeftMargin + Shift / 2 + Gap / 2 + i * (PointPerBar + Gap),
                          ImageHeight - BottomMargin -
                          (CurrentDiagramData[i].CoorYdouble - LowestScaleValue) * PointPerOneValueAfter,
                          PointPerBar,
                          CurrentDiagramData[i].CoorYdouble * PointPerOneValueAfter);
        else
          rect = new Rect(LeftMargin + Shift / 2 + Gap / 2 + i * (PointPerBar + Gap),
                          ImageHeight - BottomMargin - (0 - LowestScaleValue) * PointPerOneValueAfter,
                          PointPerBar,
                          -CurrentDiagramData[i].CoorYdouble * PointPerOneValueAfter);

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
          var percent = (int)(horizontal * 100 / ImageWidth);
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


    public int PointToBar(Point point, out int leftBar, out bool byHeight)
    {
      leftBar = -1;
      byHeight = false;
      double margin = LeftMargin + Shift / 2 + Gap / 2;
      double d = point.X - margin;
      if (d < 0) return -1; // мышь левее самого левого столбца

      var count = (int)Math.Floor(d / PointPerDate);
      var rest = d - count * PointPerDate;
      if (rest < PointPerBar && count < CurrentDiagramData.Count)
      {
        var barHeight = Y0 - PointPerOneValueAfter * CurrentDiagramData[count].CoorYdouble;
        if (barHeight < Y0) byHeight = barHeight < point.Y && point.Y < Y0;
        else byHeight = barHeight > point.Y && point.Y > Y0;
        return count; // мышь попала на столбец по горизонтали
      }
      leftBar = count >= CurrentDiagramData.Count ? CurrentDiagramData.Count -1 : count;
      return -1; // мышь не попала на столбец по горизонтали, слева кто-то есть
    }

    public int PointToBar(Point point, out int leftBar)
    {
      bool useless;
      return PointToBar(point, out leftBar, out useless);
    }

    public int PointToBar(Point point)
    {
      int useless;
      return PointToBar(point, out useless);
    }

    public int GetStartBarNumber(Point point)
    {
      int leftBar;
      var startBarNumber = PointToBar(point, out leftBar); 
      if (startBarNumber == -1) startBarNumber = ++leftBar;
      return startBarNumber;
    }

    public int GetFinishBarNumber(Point point)
    {
      int leftBar;
      var finishBarNumber = PointToBar(point, out leftBar); 
      if (finishBarNumber == -1) finishBarNumber = leftBar != -1 ? leftBar : 0;
      return finishBarNumber;

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
      var numberFrom = GetStartBarNumber(leftTop);
      var numberTo = GetFinishBarNumber(rightBottom);
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

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
      Point pt = e.GetPosition(this);
      int barLeft;
      bool isOverBar;
      var bar = PointToBar(pt, out barLeft, out isOverBar);

      if (isOverBar)
           StatusBar.Text = string.Format("  {0}:{1}   {2:MMMM yyyy} {3:0}$",pt.X,pt.Y, CurrentDiagramData[bar].CoorXdate, CurrentDiagramData[bar].CoorYdouble);
      else if (bar != -1)
           StatusBar.Text = string.Format("  {0}:{1}   Mouse pointer missed {2}th bar by height",pt.X,pt.Y, bar+1);
      else
        StatusBar.Text = string.Format("  {0}:{1}   Mouse pointer is to the right of {2}th bar", pt.X, pt.Y, barLeft+1);

    }

  }
}
