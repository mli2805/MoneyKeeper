﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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

      StatusBar.Text = "Ctrl+LeftButton - сдвиг изображения; LeftButton - зум прямоугольника";
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

    #region wrappers for pan and zoom functions

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
      CurrentSeriesUnited.DiagramData = _diagramDataPanAndZoomer.FindLimitsForRect(CurrentSeriesUnited.DiagramData, Calculator, leftTop, rightBottom);
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

    #region keyboard and mouse events handlers

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.A && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) ShowAll();
      if (e.Key == Key.F5) Draw();
    }

    private Point _mouseLeftButtonDownPoint;

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      _mouseLeftButtonDownPoint = e.GetPosition(this);
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
          SortPointsForRect(ref _mouseLeftButtonDownPoint, ref pt);
          ZoomRectFromDiagramData(_mouseLeftButtonDownPoint, pt);
        }
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
      ZoomDiagram(e.Delta);
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
      var pt = e.GetPosition(this);
      var hintCreator = new DiagramHintCreator(AllDiagramData, CurrentSeriesUnited.DiagramData, GroupInterval, _diagramMode, Calculator);
      string context;
      Brush backgroundBrush;
      if (hintCreator.CreateHint(pt, out context, out backgroundBrush))
      {
        BarHint.IsOpen = true;
        BarHint.HorizontalOffset = pt.X;
        BarHint.VerticalOffset = pt.Y - 5;

        BarHintText.Background = backgroundBrush;
        BarHintText.Text = context;
      }
      else BarHint.IsOpen = false;
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

    private void SortPointsForRect(ref Point a, ref Point b)
    {
      if (a.X > b.X)
      {
        var temp = a.X;
        a.X = b.X;
        b.X = temp;
      }
      if (a.Y > b.Y)
      {
        var temp = a.Y;
        a.Y = b.Y;
        b.Y = temp;
      }
    }

  }
}
