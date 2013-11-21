using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Keeper.Utils;
using Keeper.Utils.Diagram;

namespace Keeper.Controls
{
  public partial class BarDiagramControl
  {
    #region DependencyProperties

    public static readonly DependencyProperty AllDiagramDataProperty =
      DependencyProperty.Register("AllDiagramData", typeof(DiagramData),
                                  typeof(BarDiagramControl), new FrameworkPropertyMetadata(new DiagramData()));

    public DiagramData AllDiagramData
    {
      get { return (DiagramData)GetValue(AllDiagramDataProperty); }
      set { SetValue(AllDiagramDataProperty, value); }
    }

    #endregion

    public DiagramSeriesUnited AllSeriesUnited { get; set; }
    public DiagramSeriesUnited CurrentSeriesUnited { get; set; }
    private Every _groupInterval;
    private DiagramMode _diagramMode;

    private DateTime _minDate, _maxDate;
    private double _minValue, _maxValue;

    public DrawingCalculationData CalculationData { get; set; }
    public DrawingGroup DrawingGroup = new DrawingGroup();
    public DrawingImage ImageSource { get; set; }

    #region class essential methods

    public BarDiagramControl()
    {
      InitializeComponent();
      CalculationData = new DrawingCalculationData(this);

      Loaded += BarDiagramControlOnLoaded;
    }

    public void Connect(int connectionId, object target)
    {
      throw new NotImplementedException();
    }

    void BarDiagramControlOnLoaded(object sender, RoutedEventArgs e)
    {
      AllSeriesUnited = new DiagramSeriesUnited();
      CombineAllSeries();
      if (AllSeriesUnited.SeriesCount == 0) return;
      _groupInterval = AllDiagramData.TimeInterval;
      _diagramMode = AllDiagramData.Mode;
      CurrentSeriesUnited = new DiagramSeriesUnited(AllSeriesUnited);

      ImageSource = DrawCurrentDiagram(CalculationData);
      DiagramImage.Source = ImageSource;

      var window = Window.GetWindow(this);
      if (window != null) window.KeyDown += OnKeyDown;
    }

    #endregion 

    #region data processing methods

    /// <summary>
    /// На входе AllDiagramData, надо преобразовать в структуру, 
    /// где одной дате соответствуют значения для разных серий - List of DiagramDate
    /// </summary>
    private void CombineAllSeries()
    {
      AllSeriesUnited = new DiagramSeriesUnited();
      foreach (var diagramSeries in AllDiagramData.Data)
      {
        AllSeriesUnited.Add(diagramSeries);
      }
    }

    private void GetDataInLineLimits()
    {
      _minDate = CurrentSeriesUnited.DiagramData.ElementAt(0).Key;
      _maxDate = CurrentSeriesUnited.DiagramData.Last().Key;

      switch (_diagramMode)
      {
        case DiagramMode.BarHorizontal:
          // это вариант , когда столбцы разных серий стоят рядом
          _minValue = CurrentSeriesUnited.DiagramData.Values.Min(l => l.Min());
          _maxValue = CurrentSeriesUnited.DiagramData.Values.Max(l => l.Max());
          break;

        case DiagramMode.BarVertical:
        case DiagramMode.Line:
          // ряд серий отрицательные, либо даже просто значение отрицательное в положительной серии
          _minValue = _maxValue = 0;
          foreach (var day in CurrentSeriesUnited.DiagramData)
          {
            var plus = day.Value.Where(values => values > 0).Sum();
            var minus = day.Value.Where(values => values < 0).Sum();
            if (plus > _maxValue) _maxValue = plus;
            if (minus < _minValue) _minValue = minus;
          }
          break;
      }

      if (_maxValue > 0 && _minValue > 0) _minValue = 0;
      if (_maxValue < 0 && _minValue < 0) _maxValue = 0;
    }

    #endregion

    #region Drawing implementation

    public DrawingImage DrawCurrentDiagram(DrawingCalculationData cd)
    {
      GetDataInLineLimits();

      DrawingGroup.Children.Add(FullDiagramBackground(cd));
      DrawingGroup.Children.Add(DiagramRegionBackground(cd));

      DrawingGroup.Children.Add(HorizontalAxes(cd));
      DrawingGroup.Children.Add(VerticalGridLines(cd));

      DrawingGroup.Children.Add(XAxisDashesWithMarkers(Dock.Top, cd));
      DrawingGroup.Children.Add(XAxisDashesWithMarkers(Dock.Bottom, cd));

      DrawingGroup.Children.Add(VerticalAxes(cd));

      DrawingGroup.Children.Add(YAxisDashesWithMarkers(Dock.Left, cd));
      DrawingGroup.Children.Add(YAxisDashesWithMarkers(Dock.Right, cd));
      DrawingGroup.Children.Add(HorizontalGridLines(cd));

      if (_diagramMode == DiagramMode.BarVertical) BarVerticalDiagram(cd);
      if (_diagramMode == DiagramMode.Line) LineDiagram(cd);

      return new DrawingImage(DrawingGroup);
    } 

    private GeometryDrawing FullDiagramBackground(DrawingCalculationData cd)
    {
      var geometryDrawing = new GeometryDrawing();

      var rectGeometry = new RectangleGeometry {Rect = new Rect(0, 0, cd.ImageWidth, cd.ImageHeight)};
      geometryDrawing.Geometry = rectGeometry;
      geometryDrawing.Brush = Brushes.LightYellow; // Кисть закраски

      return geometryDrawing;
    }

    private GeometryDrawing DiagramRegionBackground(DrawingCalculationData cd)
    {
    var geometryDrawing = new GeometryDrawing();

      var rectGeometry = new RectangleGeometry
      {
        Rect = new Rect(cd.LeftMargin, cd.TopMargin, cd.ImageWidth - cd.LeftMargin - cd.RightMargin,
                        cd.ImageHeight - cd.TopMargin - cd.BottomMargin)
      };
      geometryDrawing.Geometry = rectGeometry;
      geometryDrawing.Brush = Brushes.White; // Кисть закраски

      return geometryDrawing;
    }

    private GeometryDrawing HorizontalAxes(DrawingCalculationData cd)
    {
      var geometryGroup = new GeometryGroup();
      var bottomAxis = new LineGeometry(new Point(cd.LeftMargin, cd.ImageHeight - cd.BottomMargin),
                                        new Point(cd.ImageWidth - cd.RightMargin, cd.ImageHeight - cd.BottomMargin));
      geometryGroup.Children.Add(bottomAxis);

      var topAxis = new LineGeometry(new Point(cd.LeftMargin, cd.TopMargin),
                                     new Point(cd.ImageWidth - cd.RightMargin, cd.TopMargin));
      geometryGroup.Children.Add(topAxis);

      return new GeometryDrawing { Geometry = geometryGroup, Pen = new Pen(Brushes.Black, 1) };
    }

    private GeometryDrawing VerticalGridLines(DrawingCalculationData cd)
    {
      var geometryGroupGridlines = new GeometryGroup();

      const double minPointBetweenMarkedDivision = 50;
      var markedDash = (int)Math.Ceiling(minPointBetweenMarkedDivision / cd.PointPerDate);

      for (var i = 0; i < CurrentSeriesUnited.DiagramData.Count; i++)
      {
        if (i % markedDash == 0)
        {
          var gridline =
            new LineGeometry(new Point(cd.LeftMargin + cd.Shift / 2 + cd.PointPerDate * (i + 0.5) - 1, cd.TopMargin + 5),
                             new Point(cd.LeftMargin + cd.Shift / 2 + cd.PointPerDate * (i + 0.5) - 1,
                                       cd.ImageHeight - cd.BottomMargin - 5));
          geometryGroupGridlines.Children.Add(gridline);
        }
      }

      var geometryDrawingGridlines = new GeometryDrawing { Geometry = geometryGroupGridlines, Pen = new Pen(Brushes.LightGray, 1) };
      DrawingGroup.Children.Add(geometryDrawingGridlines);
      return geometryDrawingGridlines;
    }

    private GeometryDrawing XAxisDashesWithMarkers(Dock flag, DrawingCalculationData cd)
    {
      const double minPointBetweenMarkedDivision = 50;
      var markedDash = (int)Math.Ceiling(minPointBetweenMarkedDivision / cd.PointPerDate);

      var geometryGroupDashesAndMarks = new GeometryGroup();

      for (var i = 0; i < CurrentSeriesUnited.DiagramData.Count; i++)
      {
        var dashY = flag == Dock.Bottom ? cd.ImageHeight - cd.BottomMargin : cd.TopMargin;
        var dash = new LineGeometry(new Point(cd.LeftMargin + cd.Shift / 2 + cd.PointPerDate * (i + 0.5), dashY - 5),
                                    new Point(cd.LeftMargin + cd.Shift / 2 + cd.PointPerDate * (i + 0.5), dashY + 5));
        geometryGroupDashesAndMarks.Children.Add(dash);

        if (i % markedDash == 0)
        {
          var markY = flag == Dock.Bottom ? cd.ImageHeight : cd.TopMargin;
          var mark = String.Format("{0:M/yyyy} ", CurrentSeriesUnited.DiagramData.ElementAt(i).Key);
          var formattedText = new FormattedText(mark, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                                new Typeface("Times New Roman"), 12, Brushes.Black);
          var geometry =
            formattedText.BuildGeometry(new Point(cd.LeftMargin + cd.Shift / 2 + cd.PointPerDate * (i + 0.5) - 18, markY - 20));
          geometryGroupDashesAndMarks.Children.Add(geometry);
        }
      }

      var geometryDrawingDashesAndMarks = new GeometryDrawing { Geometry = geometryGroupDashesAndMarks, Pen = new Pen(Brushes.Black, 1) };
      DrawingGroup.Children.Add(geometryDrawingDashesAndMarks);
      return geometryDrawingDashesAndMarks;
    }

    private GeometryDrawing VerticalAxes(DrawingCalculationData cd)
    {
      var geometryGroup = new GeometryGroup();
      var leftAxis = new LineGeometry(new Point(cd.LeftMargin, cd.TopMargin),
                                      new Point(cd.LeftMargin, cd.ImageHeight - cd.BottomMargin));
      geometryGroup.Children.Add(leftAxis);
      var rightAxis = new LineGeometry(new Point(cd.ImageWidth - cd.RightMargin, cd.TopMargin),
                                       new Point(cd.ImageWidth - cd.RightMargin, cd.ImageHeight - cd.BottomMargin));
      geometryGroup.Children.Add(rightAxis);

      var geometryDrawing = new GeometryDrawing { Geometry = geometryGroup, Pen = new Pen(Brushes.Black, 1) };
      return geometryDrawing;
    }

    private GeometryDrawing HorizontalGridLines(DrawingCalculationData cd)
    {
      var geometryGroupGridlines = new GeometryGroup();
      for (var i = 0; i <= cd.Divisions; i++)
      {
        var gridline = new LineGeometry(new Point(cd.LeftMargin + 5, cd.ImageHeight - cd.BottomMargin - cd.PointPerScaleStep * i),
                                        new Point(cd.ImageWidth - cd.LeftMargin - 5,
                                                  cd.ImageHeight - cd.BottomMargin - cd.PointPerScaleStep * i));
        if (i != 0 && i != cd.Divisions) geometryGroupGridlines.Children.Add(gridline);
      }

      return new GeometryDrawing { Geometry = geometryGroupGridlines, Pen = new Pen(Brushes.LightGray, 1) };
    }

    private GeometryDrawing YAxisDashesWithMarkers(Dock flag, DrawingCalculationData cd)
    {
      const double minPointBetweenDivision = 35;

      double pointPerOneValueBefore = _maxValue.Equals(_minValue) ? 
        0 : (cd.ImageHeight - cd.TopMargin - cd.BottomMargin) / (_maxValue - _minValue);
      double valuesPerDivision = (minPointBetweenDivision > pointPerOneValueBefore) ?
        Math.Ceiling(minPointBetweenDivision / pointPerOneValueBefore) : minPointBetweenDivision / pointPerOneValueBefore;
      double zeros = Math.Floor(Math.Log10(valuesPerDivision));
      cd.AccurateValuesPerDivision = Math.Ceiling(valuesPerDivision / Math.Pow(10, zeros)) * Math.Pow(10, zeros);
      cd.FromDivision = Convert.ToInt32(Math.Floor(_minValue / cd.AccurateValuesPerDivision));

      var temp = Convert.ToInt32(Math.Ceiling((_maxValue - _minValue) / cd.AccurateValuesPerDivision));
      if ((cd.FromDivision + temp) * cd.AccurateValuesPerDivision < _maxValue) temp++;
      cd.Divisions = temp;

      var geometryGroupDashesAndMarks = new GeometryGroup();
      for (var i = 0; i <= cd.Divisions; i++)
      {
        var dashX = flag == Dock.Left ? cd.LeftMargin : cd.ImageWidth - cd.LeftMargin;
        var dash = new LineGeometry(new Point(dashX - 5, cd.ImageHeight - cd.BottomMargin - cd.PointPerScaleStep * i),
                                    new Point(dashX + 5, cd.ImageHeight - cd.BottomMargin - cd.PointPerScaleStep * i));
        geometryGroupDashesAndMarks.Children.Add(dash);

        var markX = flag == Dock.Left ? cd.LeftMargin - 40 : cd.ImageWidth - cd.RightMargin + 15;
        var mark = String.Format("{0} ", (i + cd.FromDivision) * cd.AccurateValuesPerDivision);
        var formattedText = new FormattedText(mark, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                              new Typeface("Times New Roman"), 12, Brushes.Black);
        var geometry =
          formattedText.BuildGeometry(new Point(markX, cd.ImageHeight - cd.BottomMargin - cd.PointPerScaleStep * i - 7));
        geometryGroupDashesAndMarks.Children.Add(geometry);
      }

      return new GeometryDrawing { Geometry = geometryGroupDashesAndMarks, Pen = new Pen(Brushes.Black, 1) };
    }

    private void LineDiagram(DrawingCalculationData cd)
    {
      var geometryGroup = new GeometryGroup();
      int j = 0;
      for (int i = 0; i < CurrentSeriesUnited.DiagramData.Count - 1; i++)
      {
        var line = new LineGeometry(
          new Point((CurrentSeriesUnited.DiagramData.ElementAt(i).Key - _minDate).Days * cd.PointPerDate + cd.LeftMargin,
                     cd.ImageHeight - cd.BottomMargin - (CurrentSeriesUnited.DiagramData.ElementAt(i).Value[j] - cd.LowestScaleValue) * cd.PointPerOneValueAfter),
          new Point((CurrentSeriesUnited.DiagramData.ElementAt(i + 1).Key - _minDate).Days * cd.PointPerDate + cd.LeftMargin,
                     cd.ImageHeight - cd.BottomMargin - (CurrentSeriesUnited.DiagramData.ElementAt(i + 1).Value[j] - cd.LowestScaleValue) * cd.PointPerOneValueAfter));
        geometryGroup.Children.Add(line);
      }
      var geometryDrawing = new GeometryDrawing { Geometry = geometryGroup, Pen = new Pen(Brushes.LimeGreen, 1) };
      DrawingGroup.Children.Add(geometryDrawing);
    }

    private void BarVerticalDiagram(DrawingCalculationData cd)
    {
      var positiveGeometryGroups = new List<GeometryGroup>();
      var negativeGeometryGroups = new List<GeometryGroup>();
      for (var i = 0; i < CurrentSeriesUnited.SeriesCount; i++)
      {
        positiveGeometryGroups.Add(new GeometryGroup());
        negativeGeometryGroups.Add(new GeometryGroup());
      }

      for (var i = 0; i < CurrentSeriesUnited.DiagramData.Count; i++)
      {
        var oneDay = CurrentSeriesUnited.DiagramData.ElementAt(i).Value;
        double hasAlreadyPlus = 0;
        double hasAlreadyMinus = 0;
        for (var j = 0; j < CurrentSeriesUnited.SeriesCount; j++)
        {
          if (oneDay[j].Equals(0)) continue;
          Rect rect;
          if (oneDay[j] > 0)
          {
            rect = new Rect(
              cd.LeftMargin + cd.Shift / 2 + cd.Gap / 2 + i * (cd.PointPerBar + cd.Gap),
              cd.ImageHeight - cd.BottomMargin - (oneDay[j] + hasAlreadyPlus - cd.LowestScaleValue) * cd.PointPerOneValueAfter,
              cd.PointPerBar,
              oneDay[j] * cd.PointPerOneValueAfter);
            positiveGeometryGroups[j].Children.Add(new RectangleGeometry(rect));
            hasAlreadyPlus += oneDay[j];
          }
          else
          {
            rect = new Rect(
              cd.LeftMargin + cd.Shift / 2 + cd.Gap / 2 + i * (cd.PointPerBar + cd.Gap),
              cd.ImageHeight - cd.BottomMargin - (hasAlreadyMinus - cd.LowestScaleValue) * cd.PointPerOneValueAfter,
              cd.PointPerBar,
              -oneDay[j] * cd.PointPerOneValueAfter);
            negativeGeometryGroups[j].Children.Add(new RectangleGeometry(rect));
            hasAlreadyMinus += oneDay[j];
          }
        }
      }

      for (var i = 0; i < CurrentSeriesUnited.SeriesCount; i++)
      {
        var positiveGeometryDrawing = new GeometryDrawing
        {
          Geometry = positiveGeometryGroups[i],
          Brush = AllDiagramData.Data[i].PositiveBrushColor,
          Pen = new Pen(AllDiagramData.Data[i].PositiveBrushColor, 1)
        };

        DrawingGroup.Children.Add(positiveGeometryDrawing);

        var negativeGeometryDrawing = new GeometryDrawing
        {
          Geometry = negativeGeometryGroups[i],
          Brush = AllDiagramData.Data[i].NegativeBrushColor,
          Pen = new Pen(AllDiagramData.Data[i].NegativeBrushColor, 1)
        };

        DrawingGroup.Children.Add(negativeGeometryDrawing);
      }
    }

    #endregion

    private bool ChangeDiagramData(DiagramDataChangeMode mode, int horizontal, int vertical)
    {
      int shiftDateRange;
      switch (mode)
      {
        case DiagramDataChangeMode.ZoomIn:
          if (CurrentSeriesUnited.DiagramData.Count < 4) return true;
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
        case DiagramDataChangeMode.ZoomOut:
          shiftDateRange = (_maxDate - _minDate).Days * horizontal / 100;
          if (shiftDateRange < 31) shiftDateRange = 31;
          _minDate = _minDate.AddDays(-shiftDateRange);
          _maxDate = _maxDate.AddDays(shiftDateRange);
          break;
        case DiagramDataChangeMode.Move:
          var deltaMonthes = (int)Math.Round(Math.Abs(horizontal / CalculationData.PointPerDate));
          if (horizontal < 0) // двигаем влево
          {
            var newMaxDate = _maxDate.AddMonths(deltaMonthes);
            if (newMaxDate > AllSeriesUnited.DiagramData.Last().Key)
            {
              newMaxDate = AllSeriesUnited.DiagramData.Last().Key;
              deltaMonthes = (int)Math.Round((newMaxDate - _maxDate).TotalDays / 30);
            }
            _maxDate = newMaxDate;
            _minDate = _minDate.AddMonths(deltaMonthes);
          }
          else // вправо
          {
            var newMinDate = _minDate.AddMonths(-deltaMonthes);
            if (newMinDate < AllSeriesUnited.DiagramData.ElementAt(0).Key)
            {
              newMinDate = AllSeriesUnited.DiagramData.ElementAt(0).Key;
              deltaMonthes = (int)Math.Round((_minDate - newMinDate).TotalDays / 30);
            }
            _minDate = newMinDate;
            _maxDate = _maxDate.AddMonths(-deltaMonthes);
          }
          break;
        case DiagramDataChangeMode.ZoomInRect:

          break;
      }
      ExtractDataBetweenLimits();
      return true;
    }

    public void ExtractDataBetweenLimits()
    {
      CurrentSeriesUnited.DiagramData.Clear();
      foreach (var day in AllSeriesUnited.DiagramData)
      {
        if (day.Key >= _minDate && day.Key <= _maxDate)
        {
          CurrentSeriesUnited.DiagramData.Add(day.Key, day.Value);
        }
      }
    }


    #region преобразует точки к данным диаграммы и запускает перерисовку
    // привязан к типу диаграммы (в данном случае - столбцовая, время - значение)

    public int PointToBar(Point point, out int leftBar, out bool byHeight)
    {
      leftBar = -1;
      byHeight = false;
      double margin = CalculationData.LeftMargin + CalculationData.Shift / 2 + CalculationData.Gap / 2;
      double d = point.X - margin;
      if (d < 0) return -1; // мышь левее самого левого столбца

      var count = (int)Math.Floor(d / CalculationData.PointPerDate);
      var rest = d - count * CalculationData.PointPerDate;
      if (rest < CalculationData.PointPerBar && count < CurrentSeriesUnited.DiagramData.Count)
      {
        var barHeight = CalculationData.Y0 - CalculationData.PointPerOneValueAfter * CurrentSeriesUnited.DiagramData.ElementAt(count).Value.Sum();
        if (barHeight < CalculationData.Y0) byHeight = barHeight < point.Y && point.Y < CalculationData.Y0;
        else byHeight = barHeight > point.Y && point.Y > CalculationData.Y0;
        return count; // мышь попала на столбец по горизонтали
      }
      leftBar = count >= CurrentSeriesUnited.DiagramData.Count ? CurrentSeriesUnited.DiagramData.Count - 1 : count;
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

    public void ZoomDiagram(DiagramDataChangeMode param, int horizontal, int vertical)
    {
      if (ChangeDiagramData(param, horizontal, vertical)) ImageSource = DrawCurrentDiagram(CalculationData);
    }

    public void MoveDiagramData(DiagramDataChangeMode param, int horizontal, int vertical)
    {
      if (ChangeDiagramData(param, horizontal, vertical)) ImageSource = DrawCurrentDiagram(CalculationData);
    }

    public void ZoomRectDiagram(Point leftTop, Point rightBottom)
    {
      var numberFrom = GetStartBarNumber(leftTop);
      var numberTo = GetFinishBarNumber(rightBottom);
      if (numberTo - numberFrom < 3) return;
      var nuevoCurrentDiagramData = new SortedList<DateTime, List<double>>();
      for (int i = numberFrom; i <= numberTo; i++)
      {
        nuevoCurrentDiagramData.Add(CurrentSeriesUnited.DiagramData.ElementAt(i).Key,
             CurrentSeriesUnited.DiagramData.ElementAt(i).Value);
      }
      CurrentSeriesUnited.DiagramData = nuevoCurrentDiagramData;
      ImageSource = DrawCurrentDiagram(CalculationData);
    }

    public void ShowAllDiagram()
    {
      CurrentSeriesUnited.DiagramData = new SortedList<DateTime, List<double>>(AllSeriesUnited.DiagramData);
      ImageSource = DrawCurrentDiagram(CalculationData);
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
        MoveDiagramData(DiagramDataChangeMode.Move, (int)(pt.X - _mouseRightButtonDownPoint.X),
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
      ZoomDiagram(e.Delta < 0 ? DiagramDataChangeMode.ZoomIn : DiagramDataChangeMode.ZoomOut, 10, 0);
    }

    #endregion

    #region popup menu

    private void GroupAllData(Every period)
    {
      _groupInterval = period;

      var groupedData = new SortedList<DateTime, List<double>>();
      var onePair = AllSeriesUnited.DiagramData.ElementAt(0);
      for (var p = 1; p < AllSeriesUnited.DiagramData.Count; p++)
      {
        var pair = AllSeriesUnited.DiagramData.ElementAt(p);
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
      AllSeriesUnited.DiagramData = new SortedList<DateTime, List<double>>(groupedData);
    }

    private void ChangeDiagramForNewGrouping(Every groupPeriod)
    {
      CombineAllSeries();
      GroupAllData(groupPeriod);

      if (groupPeriod == Every.Year) DefineYearsLimits();
      else _maxDate = new DateTime(_maxDate.Year, 12, 31);

      ExtractDataBetweenLimits();
      ImageSource = DrawCurrentDiagram(CalculationData);
    }

    private void GroupByMonthes(object sender, RoutedEventArgs e)
    {
      if (_groupInterval == Every.Month) return;
      ChangeDiagramForNewGrouping(Every.Month);
    }

    private void GroupByYears(object sender, RoutedEventArgs e)
    {
      if (_groupInterval == Every.Year) return;
      ChangeDiagramForNewGrouping(Every.Year);
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
      if (e.Key == Key.F5) ImageSource = DrawCurrentDiagram(CalculationData);
    }

    private Brush DefineBarHintBackground(int barNumber)
    {
      if (AllDiagramData.Data.Count == 1) return CurrentSeriesUnited.DiagramData.ElementAt(barNumber).Value[0] > 0 ?
                                                                       Brushes.Azure : Brushes.LavenderBlush;
      return Brushes.White;
    }

    private string CreateBarHintContent(int barNumber)
    {
      var thisBar = CurrentSeriesUnited.DiagramData.ElementAt(barNumber);
      var content = _groupInterval == Every.Month
                          ? "  {0:MMMM yyyy}  "
                          : "  {0:yyyy} год  ";

      if (AllDiagramData.Data.Count == 1)
      {
        content += "\n  {1:0} usd ";
        return string.Format(content,thisBar.Key,thisBar.Value[0]);
      }

      var i = 0;
      content = string.Format(content, thisBar.Key);
      foreach (var series in AllDiagramData.Data)
      {
        if (!thisBar.Value[i].Equals(0))
          content += string.Format("\n  {0} = {1:0} usd  ", series.Name, thisBar.Value[i]);
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
