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

    public DiagramDataSeriesUnited AllSeriesUnited { get; set; }
    public DiagramDataSeriesUnited CurrentSeriesUnited { get; set; }
    private Every _groupInterval;
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
    private DiagramDataPanAndZoomer _diagramDataPanAndZoomer;

    public DiagramDrawingCalculator Calculator { get; set; }

    #region class essential methods

    public BarDiagramControl()
    {
      InitializeComponent();
      Calculator = new DiagramDrawingCalculator(this);

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
      AllSeriesUnited = new DiagramDataSeriesUnited(AllDiagramData);
      if (AllSeriesUnited.SeriesCount == 0) return;
      GroupInterval = AllDiagramData.TimeInterval;
      _diagramMode = AllDiagramData.Mode;

      CurrentSeriesUnited = new DiagramDataSeriesUnited(AllSeriesUnited);
      DiagramDataExtremums = CurrentSeriesUnited.FindDataExtremums(_diagramMode);
      _diagramDrawer = new DiagramDrawer();
      _diagramDataPanAndZoomer = new DiagramDataPanAndZoomer();
      Draw();
      var window = Window.GetWindow(this);
      if (window != null) window.KeyDown += OnKeyDown;
    }

    #endregion

    private void Draw()
    {
      DiagramImage.Source =
        _diagramDrawer.Draw(CurrentSeriesUnited, DiagramDataExtremums, Calculator, _diagramMode, GroupInterval);
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
      double margin = Calculator.LeftMargin + Calculator.Shift / 2 + Calculator.Gap / 2;
      double d = point.X - margin;
      if (d < 0) return -1; // мышь левее самого левого столбца

      var count = (int)Math.Floor(d / Calculator.PointPerDataElement);
      var rest = d - count * Calculator.PointPerDataElement;
      if (rest < Calculator.PointPerBar && count < CurrentSeriesUnited.DiagramData.Count)
      {
        var barHeight = Calculator.Y0 - Calculator.PointPerOneValueAfter * CurrentSeriesUnited.DiagramData.ElementAt(count).Value.Sum();
        if (barHeight < Calculator.Y0) byHeight = barHeight < point.Y && point.Y < Calculator.Y0;
        else byHeight = barHeight > point.Y && point.Y > Calculator.Y0;
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

    public void ZoomDiagram(int delta)
    {
      if (!_diagramDataPanAndZoomer.ZoomLimits(CurrentSeriesUnited, GroupInterval, delta, ref DiagramDataExtremums)) return;
      ExtractDataBetweenLimits();
      DiagramDataExtremums = CurrentSeriesUnited.FindDataExtremums(_diagramMode);
      Draw();
    }

 public void MoveDiagramData(int horizontalPoints, int verticalPoints)
    {
      if (!_diagramDataPanAndZoomer.MoveLimits(AllSeriesUnited, Calculator, horizontalPoints, verticalPoints, ref DiagramDataExtremums)) return;
      ExtractDataBetweenLimits();
      DiagramDataExtremums = CurrentSeriesUnited.FindDataExtremums(_diagramMode);
      Draw();
    }

    public void ZoomRectFromDiagramData(Point leftTop, Point rightBottom)
    {
      _diagramDataPanAndZoomer.FindLimitsForRect(CurrentSeriesUnited, leftTop, rightBottom, ref DiagramDataExtremums);
      ExtractDataBetweenLimits();
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

    private Point _mouseLeftButtonDownPoint;

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      _mouseLeftButtonDownPoint = e.GetPosition(this);
    }

    private void ReSortPointsForRect(ref Point a, ref Point b)
    {
      if (a.X < b.X)
      {
        var temp = a.X;
        a.X = b.X;
        b.X = temp;
      }
      if (a.Y < b.Y)
      {
        var temp = a.Y;
        a.Y = b.Y;
        b.Y = temp;
      }
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      var pt = e.GetPosition(this);
      if (pt == _mouseLeftButtonDownPoint) return;
      if (Math.Abs(pt.X - _mouseLeftButtonDownPoint.X) < 4 && Math.Abs(pt.Y - _mouseLeftButtonDownPoint.Y) < 4) ShowAll();
      else
      if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) // c Ctrl это сдвиг изображения
        MoveDiagramData((int)(pt.X - _mouseLeftButtonDownPoint.X), (int)(pt.Y - _mouseLeftButtonDownPoint.Y));
      else
      // без Ctrl это выделение прямоугольника для зума
      {
        ReSortPointsForRect(ref _mouseLeftButtonDownPoint, ref pt);
        ZoomRectFromDiagramData(_mouseLeftButtonDownPoint, pt);
      }
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
      ZoomDiagram(e.Delta);
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
      AllSeriesUnited = new DiagramDataSeriesUnited(AllDiagramData);
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
