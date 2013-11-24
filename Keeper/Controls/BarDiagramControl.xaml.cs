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
    public Every GroupInterval
    {
      get { return _groupInterval; }
      set
      {
        _groupInterval = value;
        SetPopupMenuItemsVisibility(value);
      }
    }

    private void SetPopupMenuItemsVisibility(Every groupInterval)
    {
      switch (groupInterval)
      {
        case Every.Day:
          ItemShowThisYear.Visibility = Visibility.Visible;
          ItemShowLast12Monthes.Visibility = Visibility.Visible;
          ItemShowThisMonth.Visibility = Visibility.Visible;
          ItemGroupByMonthes.Visibility = Visibility.Collapsed;
          ItemGroupByYears.Visibility = Visibility.Collapsed;
          break;
        case Every.Month:
          ItemShowThisYear.Visibility = Visibility.Visible;
          ItemShowLast12Monthes.Visibility = Visibility.Visible;
          ItemShowThisMonth.Visibility = Visibility.Collapsed;
          ItemGroupByMonthes.Visibility = Visibility.Collapsed;
          ItemGroupByYears.Visibility = Visibility.Visible;
          break;
        case Every.Year:
          ItemShowThisYear.Visibility = Visibility.Collapsed;
          ItemShowLast12Monthes.Visibility = Visibility.Collapsed;
          ItemShowThisMonth.Visibility = Visibility.Collapsed;
          ItemGroupByMonthes.Visibility = Visibility.Visible;
          ItemGroupByYears.Visibility = Visibility.Collapsed;
          break;
      }
    }

    private DiagramMode _diagramMode;
    public DiagramDataExtremums DiagramDataExtremums;
    private DiagramDrawer _diagramDrawer;

    public DrawingCalculationData CalculationData { get; set; }

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
      // набор серий надо преобразовать в структуру, 
      // где одной дате соответствуют значения для разных серий
      AllSeriesUnited = new DiagramSeriesUnited(AllDiagramData);
      if (AllSeriesUnited.SeriesCount == 0) return;
      GroupInterval = AllDiagramData.TimeInterval;
      _diagramMode = AllDiagramData.Mode;

      CurrentSeriesUnited = new DiagramSeriesUnited(AllSeriesUnited);
      DiagramDataExtremums = CurrentSeriesUnited.FindDataExtremums(_diagramMode);
      _diagramDrawer = new DiagramDrawer();
      Draw();
      var window = Window.GetWindow(this);
      if (window != null) window.KeyDown += OnKeyDown;
    }

    #endregion

    private void Draw()
    {
      DiagramImage.Source =
        _diagramDrawer.Draw(CurrentSeriesUnited, DiagramDataExtremums, CalculationData, _diagramMode, GroupInterval);
    }

    private bool ChangeDiagramData(DiagramDataChangeMode mode, int horizontal, int vertical)
    {
      int shiftDateRange;
      switch (mode)
      {
        case DiagramDataChangeMode.ZoomIn: // increase picture
          if (CurrentSeriesUnited.DiagramData.Count < 6) return true;
          shiftDateRange = (DiagramDataExtremums.MaxDate - DiagramDataExtremums.MinDate).Days * horizontal / 100;
          if (shiftDateRange < 31 && GroupInterval == Every.Month) shiftDateRange = 31;
          if (shiftDateRange < 366 && GroupInterval == Every.Year) shiftDateRange = 366;
          DiagramDataExtremums.MinDate = DiagramDataExtremums.MinDate.AddDays(shiftDateRange);
          DiagramDataExtremums.MaxDate = DiagramDataExtremums.MaxDate.AddDays(-shiftDateRange);
          break;
        case DiagramDataChangeMode.ZoomOut:
          shiftDateRange = (DiagramDataExtremums.MaxDate - DiagramDataExtremums.MinDate).Days * horizontal / 100;
          if (shiftDateRange < 31 && GroupInterval == Every.Month) shiftDateRange = 31;
          if (shiftDateRange < 366 && GroupInterval == Every.Year) shiftDateRange = 366;
          DiagramDataExtremums.MinDate = DiagramDataExtremums.MinDate.AddDays(-shiftDateRange);
          DiagramDataExtremums.MaxDate = DiagramDataExtremums.MaxDate.AddDays(shiftDateRange);
          break;
        case DiagramDataChangeMode.Move:
          var deltaMonthes = (int)Math.Round(Math.Abs(horizontal / CalculationData.PointPerDataElement));
          if (horizontal < 0) // двигаем влево
          {
            var newMaxDate = DiagramDataExtremums.MaxDate.AddMonths(deltaMonthes);
            if (newMaxDate > AllSeriesUnited.DiagramData.Last().Key)
            {
              newMaxDate = AllSeriesUnited.DiagramData.Last().Key;
              deltaMonthes = (int)Math.Round((newMaxDate - DiagramDataExtremums.MaxDate).TotalDays / 30);
            }
            DiagramDataExtremums.MaxDate = newMaxDate;
            DiagramDataExtremums.MinDate = DiagramDataExtremums.MinDate.AddMonths(deltaMonthes);
          }
          else // вправо
          {
            var newMinDate = DiagramDataExtremums.MinDate.AddMonths(-deltaMonthes);
            if (newMinDate < AllSeriesUnited.DiagramData.ElementAt(0).Key)
            {
              newMinDate = AllSeriesUnited.DiagramData.ElementAt(0).Key;
              deltaMonthes = (int)Math.Round((DiagramDataExtremums.MinDate - newMinDate).TotalDays / 30);
            }
            DiagramDataExtremums.MinDate = newMinDate;
            DiagramDataExtremums.MaxDate = DiagramDataExtremums.MaxDate.AddMonths(-deltaMonthes);
          }
          break;
        case DiagramDataChangeMode.ZoomInRect:

          break;
      }
      ExtractDataBetweenLimits();
      DiagramDataExtremums = CurrentSeriesUnited.FindDataExtremums(_diagramMode);
      return true;
    }

    public void ExtractDataBetweenLimits()
    {
      CurrentSeriesUnited.DiagramData.Clear();
      foreach (var day in AllSeriesUnited.DiagramData)
      {
        if (day.Key >= DiagramDataExtremums.MinDate && day.Key <= DiagramDataExtremums.MaxDate)
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

      var count = (int)Math.Floor(d / CalculationData.PointPerDataElement);
      var rest = d - count * CalculationData.PointPerDataElement;
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
      if (ChangeDiagramData(param, horizontal, vertical)) Draw();
    }

    public void MoveDiagramData(DiagramDataChangeMode param, int horizontal, int vertical)
    {
      if (ChangeDiagramData(param, horizontal, vertical)) Draw();
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
      DiagramDataExtremums = CurrentSeriesUnited.FindDataExtremums(_diagramMode);
      Draw();
    }

    public void ShowAll()
    {
      CurrentSeriesUnited.DiagramData = new SortedList<DateTime, List<double>>(AllSeriesUnited.DiagramData);
      DiagramDataExtremums = CurrentSeriesUnited.FindDataExtremums(_diagramMode);
      Draw();
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
    private Every _groupInterval;

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

      if (pt.X - _mouseLeftButtonDownPoint.X < 4 || pt.Y - _mouseLeftButtonDownPoint.Y < 4) ShowAll();
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
      GroupInterval = period;

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
      AllSeriesUnited = new DiagramSeriesUnited(AllDiagramData);
      GroupAllData(groupPeriod);

      if (groupPeriod == Every.Year) DefineYearsLimits();
      else DiagramDataExtremums.MaxDate = new DateTime(DiagramDataExtremums.MaxDate.Year, 12, 31);

      ExtractDataBetweenLimits();
      DiagramDataExtremums = CurrentSeriesUnited.FindDataExtremums(_diagramMode);
      Draw();
    }


    private void ShowAllRange(object sender, RoutedEventArgs e)
    {
      ShowAll();
    }

    private void ShowThisYear(object sender, RoutedEventArgs e)
    {
      var date = AllSeriesUnited.DiagramData.Last().Key;
      DiagramDataExtremums.MinDate = new DateTime(date.Year, 1, 1);
      DiagramDataExtremums.MaxDate = date;
      ExtractDataBetweenLimits();
      DiagramDataExtremums = CurrentSeriesUnited.FindDataExtremums(_diagramMode);
      Draw();
    }

    private void ShowLast12Monthes(object sender, RoutedEventArgs e)
    {
      var date = AllSeriesUnited.DiagramData.Last().Key;
      DiagramDataExtremums.MinDate = new DateTime(date.AddMonths(-11).Year, date.AddMonths(-11).Month, 1);
      DiagramDataExtremums.MaxDate = date;
      ExtractDataBetweenLimits();
      DiagramDataExtremums = CurrentSeriesUnited.FindDataExtremums(_diagramMode);
      Draw();
    }

    private void ShowThisMonth(object sender, RoutedEventArgs e)
    {
      var date = AllSeriesUnited.DiagramData.Last().Key;
      DiagramDataExtremums.MinDate = new DateTime(date.Year, date.Month, 1);
      DiagramDataExtremums.MaxDate = date;
      ExtractDataBetweenLimits();
      DiagramDataExtremums = CurrentSeriesUnited.FindDataExtremums(_diagramMode);
      Draw();
    }
    private void GroupByMonthes(object sender, RoutedEventArgs e)
    {
      if (GroupInterval == Every.Month) return;
      ChangeDiagramForNewGrouping(Every.Month);
    }

    private void GroupByYears(object sender, RoutedEventArgs e)
    {
      if (GroupInterval == Every.Year) return;
      ChangeDiagramForNewGrouping(Every.Year);
    }

    private void DefineYearsLimits()
    {
      const int bottomLimit = 2002;
      var topLimit = DateTime.Today.Year;

      var startYear = DiagramDataExtremums.MinDate.Year;
      var endYear = DiagramDataExtremums.MaxDate.Year;

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

      DiagramDataExtremums.MinDate = new DateTime(startYear, 1, 1);
      DiagramDataExtremums.MaxDate = new DateTime(endYear, 12, 31);
    }
    #endregion

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.A && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) ShowAll();
      if (e.Key == Key.F5) Draw();
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
      var content = GroupInterval == Every.Month
                          ? "  {0:MMMM yyyy}  "
                          : "  {0:yyyy} год  ";

      if (AllDiagramData.Data.Count == 1)
      {
        content += "\n  {1:0} usd ";
        return string.Format(content, thisBar.Key, thisBar.Value[0]);
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

      switch (_diagramMode)
      {
        case DiagramMode.BarVertical:
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
          break;
        case DiagramMode.Line:
          break;
        default:
          BarHint.IsOpen = false;
          StatusBar.Text = "";
          break;
      }
    }

  }
}
