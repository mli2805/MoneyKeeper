using System;
using System.Collections.Generic;
using System.Globalization;
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

    public static readonly DependencyProperty DiagramModeProperty =
      DependencyProperty.Register("DiagramMode", typeof(BarDiagramMode),
                                  typeof(BarDiagramControl), new FrameworkPropertyMetadata(new BarDiagramMode()));

    public BarDiagramMode DiagramMode
    {
      get { return (BarDiagramMode) GetValue(DiagramModeProperty); }
      set { SetValue(DiagramModeProperty, value);}
    }

    public static readonly DependencyProperty AllDiagramSeriesProperty =
      DependencyProperty.Register("AllDiagramSeries", typeof (List<DiagramSeries>),
                                  typeof(BarDiagramControl), new FrameworkPropertyMetadata(new List<DiagramSeries>()));

    public List<DiagramSeries> AllDiagramSeries
    {
      get { return (List < DiagramSeries >) GetValue(AllDiagramSeriesProperty); }
      set {SetValue(AllDiagramSeriesProperty, value);}
    }
    
    public DateLineDiagramData AllData { get; set; }
    public DateLineDiagramData CurrentData { get; set; }
    private Every _oneBarPeriod;

    /// <summary>
    /// На входе AllDiagramSeries, надо преобразовать в структуру, 
    /// где одной дате соответствуют значения для разных серий - List of DiagramDate
    /// </summary>
    private void AllSeriesToAllData()
    {
      AllData = new DateLineDiagramData();
      foreach (var diagramSeries in AllDiagramSeries)
      {
        AllData.Add(diagramSeries);
      }
    }

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
      if (CurrentData.DiagramData == null) return 0;
      if (CurrentData.DiagramData.Count == 0) return 0;
      return (ImageWidth - LeftMargin - RightMargin - Shift) / CurrentData.DiagramData.Count;
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

      Loaded += BarDiagramControlOnLoaded;
    }

    void BarDiagramControlOnLoaded(object sender, RoutedEventArgs e)
    {
      _oneBarPeriod = Every.Month;
      AllData = new DateLineDiagramData();
      AllSeriesToAllData();
      if (AllData.SeriesCount == 0) return;
      CurrentData = new DateLineDiagramData(AllData);

      DrawCurrentDiagram();
      DiagramImage.Source = ImageSource;

      var window = Window.GetWindow(this);
      if (window != null) window.KeyDown += OnKeyDown; 
    }

    private void GetDataInLineLimits(BarDiagramMode mode)
    {
      _minDate = CurrentData.DiagramData.ElementAt(0).Key;
      _maxDate = CurrentData.DiagramData.Last().Key;

      switch (mode)
      {
          case BarDiagramMode.Horizontal:
          // это вариант , когда столбцы разных серий стоят рядом
          _minValue = CurrentData.DiagramData.Values.Min(l => l.Min());
          _maxValue = CurrentData.DiagramData.Values.Max(l => l.Max());
          break;

          case BarDiagramMode.Vertical:
          // Vertical , столбцы стоят один на одном и надо находить min/max суммы
          case BarDiagramMode.Butterfly:
          // Butterfly - Vertical, но ряд серий отрицательные НО их сумма НЕ должна считаться отдельно
          _minValue = CurrentData.DiagramData.Values.Min(l => l.Sum());
          _maxValue = CurrentData.DiagramData.Values.Max(l => l.Sum());
          break;
      }

      if (_minValue > 0) _minValue = 0;
    }

    #region Drawing implementation

    public void DrawCurrentDiagram()
    {
      GetDataInLineLimits(BarDiagramMode.Butterfly);

      DiagramBackground();

      HorizontalAxes();
      HorizontalAxisWithMarkers(Dock.Top);
      HorizontalAxisWithMarkers(Dock.Bottom);

      VerticalAxes();
      MarkersForVerticalAxes(Dock.Left);
      MarkersForVerticalAxes(Dock.Right);

      if (DiagramMode == BarDiagramMode.Butterfly) ButterflyDiagram();
      if (DiagramMode == BarDiagramMode.Vertical) VerticalDiagram();
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

      for (var i = 0; i < CurrentData.DiagramData.Count; i++)
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
          var mark = String.Format("{0:M/yyyy} ", CurrentData.DiagramData.ElementAt(i).Key);
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

    private void VerticalDiagram()
    {
      var positiveGeometryGroups = new List<GeometryGroup>();
      for (int i = 0; i < CurrentData.SeriesCount; i++)
      {
        positiveGeometryGroups.Add(new GeometryGroup());
      }

      for (int i = 0; i < CurrentData.DiagramData.Count; i++)
      {
        var oneDay = CurrentData.DiagramData.ElementAt(i).Value;
        double hasAlreadyHeight = 0;
        for (int j = 0; j < CurrentData.SeriesCount; j++)
        {
          if (oneDay[j].Equals(0)) continue;
          var rect = new Rect(LeftMargin + Shift / 2 + Gap / 2 + i * (PointPerBar + Gap),
                                ImageHeight - BottomMargin - (oneDay[j] + hasAlreadyHeight - LowestScaleValue) * PointPerOneValueAfter,
                                PointPerBar,
                                oneDay[j] * PointPerOneValueAfter);
          positiveGeometryGroups[j].Children.Add(new RectangleGeometry(rect));
          hasAlreadyHeight += oneDay[j];
        }
      }

      for (int i = 0; i < CurrentData.SeriesCount; i++)
      {
        var positiveGeometryDrawing = new GeometryDrawing
        {
          Geometry = positiveGeometryGroups[i],
          Brush = AllDiagramSeries[i].positiveBrushColor,
          Pen = new Pen(AllDiagramSeries[i].positiveBrushColor, 1)
        };

        DrawingGroup.Children.Add(positiveGeometryDrawing);
      }

    }

    private  void ButterflyDiagram()
    {
      var positiveGeometryGroups = new List<GeometryGroup>();
      var negativeGeometryGroups = new List<GeometryGroup>();
      for (int i = 0; i < CurrentData.SeriesCount; i++)
      {
        positiveGeometryGroups.Add(new GeometryGroup());
        negativeGeometryGroups.Add(new GeometryGroup());
      }

      for (int i = 0; i < CurrentData.DiagramData.Count; i++)
      {
        var oneDay = CurrentData.DiagramData.ElementAt(i).Value;
        for (int j = 0; j < CurrentData.SeriesCount; j++)
        {
          if (oneDay[j].Equals(0)) continue;
          Rect rect;
          if (oneDay[j] > 0)
          {
            rect = new Rect(LeftMargin + Shift / 2 + Gap / 2 + i * (PointPerBar + Gap),
                                ImageHeight - BottomMargin - (oneDay[j] - LowestScaleValue) * PointPerOneValueAfter,
                                PointPerBar,
                                oneDay[j] * PointPerOneValueAfter);
            positiveGeometryGroups[j].Children.Add(new RectangleGeometry(rect));
          }
          else
          {
            rect = new Rect(LeftMargin + Shift / 2 + Gap / 2 + i * (PointPerBar + Gap),
                            ImageHeight - BottomMargin - (0 - LowestScaleValue) * PointPerOneValueAfter,
                            PointPerBar,
                            -oneDay[j] * PointPerOneValueAfter);
            negativeGeometryGroups[j].Children.Add(new RectangleGeometry(rect));
          }
        }
      }

      for (int i = 0; i < CurrentData.SeriesCount; i++)
      {
        var positiveGeometryDrawing = new GeometryDrawing
                                      {
                                        Geometry = positiveGeometryGroups[i],
                                        Brush = AllDiagramSeries[i].positiveBrushColor,
                                        Pen = new Pen(AllDiagramSeries[i].positiveBrushColor, 1)
                                      };

        DrawingGroup.Children.Add(positiveGeometryDrawing);

        var negativeGeometryDrawing = new GeometryDrawing
        {
          Geometry = negativeGeometryGroups[i],
          Brush = AllDiagramSeries[i].negativeBrushColor,
          Pen = new Pen(AllDiagramSeries[i].negativeBrushColor, 1)
        };

        DrawingGroup.Children.Add(negativeGeometryDrawing);
      }

    }
    #endregion


    private bool ChangeDiagramData(ChangeDiagramDataMode mode, int horizontal, int vertical)
    {
      int shiftDateRange;
      switch (mode)
      {
        case ChangeDiagramDataMode.ZoomIn:
          if (CurrentData.DiagramData.Count < 4) return true;
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
          var deltaMonthes = (int)Math.Round(Math.Abs(horizontal/PointPerDate));
          if (horizontal < 0) // двигаем влево
          {
            var newMaxDate = _maxDate.AddMonths(deltaMonthes); 
            if (newMaxDate > AllData.DiagramData.Last().Key)
            {
              newMaxDate = AllData.DiagramData.Last().Key;
              deltaMonthes = (int)Math.Round((newMaxDate - _maxDate).TotalDays / 30);
            }
            _maxDate = newMaxDate;
            _minDate = _minDate.AddMonths(deltaMonthes);
          }
          else // вправо
          {
            var newMinDate = _minDate.AddMonths(-deltaMonthes); 
            if (newMinDate < AllData.DiagramData.ElementAt(0).Key)
            {
              newMinDate = AllData.DiagramData.ElementAt(0).Key;
              deltaMonthes = (int)Math.Round((_minDate - newMinDate).TotalDays / 30);
            }
            _minDate = newMinDate;
            _maxDate = _maxDate.AddMonths(-deltaMonthes);
          }
          break;
        case ChangeDiagramDataMode.ZoomInRect:

          break;
      }
      ExtractDataBetweenLimits();
      return true;
    }

    public void ExtractDataBetweenLimits()
    {
      CurrentData.DiagramData.Clear();
      foreach (var day in AllData.DiagramData)
      {
        if (day.Key >= _minDate && day.Key <= _maxDate)
        {
          CurrentData.DiagramData.Add(day.Key, day.Value);
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
      if (rest < PointPerBar && count < CurrentData.DiagramData.Count)
      {
        var barHeight = Y0 - PointPerOneValueAfter * CurrentData.DiagramData.ElementAt(count).Value.Sum();
        if (barHeight < Y0) byHeight = barHeight < point.Y && point.Y < Y0;
        else byHeight = barHeight > point.Y && point.Y > Y0;
        return count; // мышь попала на столбец по горизонтали
      }
      leftBar = count >= CurrentData.DiagramData.Count ? CurrentData.DiagramData.Count -1 : count;
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
      var nuevoCurrentDiagramData = new SortedList<DateTime, List<double>>();
      for (int i = numberFrom; i <= numberTo; i++)
      {
        nuevoCurrentDiagramData.Add(CurrentData.DiagramData.ElementAt(i).Key,
             CurrentData.DiagramData.ElementAt(i).Value);
      }
      CurrentData.DiagramData = nuevoCurrentDiagramData;
      DrawCurrentDiagram();
    }

    public void ShowAllDiagram()
    {
      CurrentData.DiagramData = new SortedList<DateTime, List<double>>(AllData.DiagramData);
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

    private void GroupAllData(Every period)
    {
      _oneBarPeriod = period;

      var groupedData = new SortedList<DateTime, List<double>>();
      var onePair = AllData.DiagramData.ElementAt(0);
      for (var p = 1; p < AllData.DiagramData.Count; p++ )
      {
        var pair = AllData.DiagramData.ElementAt(p);
        if (FunctionsWithEvery.IsTheSamePeriod(onePair.Key, pair.Key, period))
        {
          for (var i = 0; i < onePair.Value.Count; i++)
            onePair.Value[i] += pair.Value[i];
        }
        else
        {
          groupedData.Add(onePair.Key, onePair.Value);
          onePair = pair;
        }
      }
      groupedData.Add(onePair.Key, onePair.Value);
      AllData.DiagramData = new SortedList<DateTime, List<double>>(groupedData);
    }

    private void ChangeDiagramForNewGrouping(Every groupPeriod)
    {
      AllSeriesToAllData();
      GroupAllData(groupPeriod);

      if (groupPeriod == Every.Year) DefineYearsLimits();
      else _maxDate = new DateTime(_maxDate.Year, 12, 31);

      ExtractDataBetweenLimits();
      DrawCurrentDiagram();
    }

    private void GroupByMonthes(object sender, RoutedEventArgs e)
    {
      if (_oneBarPeriod == Every.Month) return;
      ChangeDiagramForNewGrouping(Every.Month);
    }

    private void GroupByYears(object sender, RoutedEventArgs e)
    {
      if (_oneBarPeriod == Every.Year) return;
      ChangeDiagramForNewGrouping(Every.Year);
    }
    private void ExtractDataBetweenLimitsWithYearsGrouping()
    {
      CurrentData.DiagramData.Clear();
      var startYearDate = new DateTime(0);
      var yearValue = new List<double>();
      foreach (var day in AllData.DiagramData)
      {
        if (day.Key < _minDate || day.Key > _maxDate) continue;

        if (startYearDate.Year != day.Key.Year)
        {
          if (startYearDate.Year != 1)
          {
            CurrentData.DiagramData.Add(startYearDate, yearValue);
          }

          startYearDate = day.Key;
          yearValue = day.Value;
        }
        else
        {
          for (int i = 0; i < day.Value.Count; i++)
          {
            if (yearValue.Count > i) yearValue[i] += day.Value[i];
            else yearValue.Add(day.Value[i]);
          }
        }
      }
      CurrentData.DiagramData.Add(startYearDate, yearValue);
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
      if (e.Key == Key.F5) DrawCurrentDiagram();
    }

    private Brush DefineBarHintBackground(int barNumber)
    {
      if (AllDiagramSeries.Count == 1) return CurrentData.DiagramData.ElementAt(barNumber).Value[0] > 0 ?
                                                                       Brushes.Azure : Brushes.LavenderBlush;
      return Brushes.White;
    }

    private string CreateBarHintContent(int barNumber)
    {
      var content = _oneBarPeriod == Every.Month
                          ? "  {0:MMMM yyyy}  "
                          : "  {0:yyyy} год  ";

      if (AllDiagramSeries.Count == 1)
      {
        content += "\n  {1:0} usd ";
        return string.Format(content,     
                        CurrentData.DiagramData.ElementAt(barNumber).Key,
                        CurrentData.DiagramData.ElementAt(barNumber).Value[0]);
      }

      int i = 0;
      content = string.Format(content, CurrentData.DiagramData.ElementAt(barNumber).Key);
      foreach (var series in AllDiagramSeries)
      {
        content += string.Format("\n  {0} - {1:0} usd  ", series.Name, CurrentData.DiagramData.ElementAt(barNumber).Value[i]);
        i++;
      }

      return content;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
      Point pt = e.GetPosition(this);
      int barLeft;
      bool isOverBar;
      var bar = PointToBar(pt, out barLeft, out isOverBar);

      if (isOverBar)
      {
        BarHint.IsOpen = true;
        BarHint.HorizontalOffset = pt.X;
        BarHint.VerticalOffset = pt.Y - 5;

        BarHintText.Background = DefineBarHintBackground(bar);
        BarHintText.Text = CreateBarHintContent(bar);
      }
      else // debug info
      {
        BarHint.IsOpen = false;
        if (bar != -1)
          StatusBar.Text = string.Format("  {0}:{1}   Mouse pointer missed {2}th bar by height", pt.X, pt.Y, bar + 1);
        else
          StatusBar.Text = string.Format("  {0}:{1}   Mouse pointer is to the right of {2}th bar", pt.X, pt.Y,
                                         barLeft + 1);
      }
    }
  }
}
