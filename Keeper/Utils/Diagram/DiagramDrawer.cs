﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Keeper.Utils.Diagram
{
  class DiagramDrawer
  {
    private DiagramSeriesUnited _diagramData;
    private DiagramDataExtremums _extremums;
    private DrawingCalculationData _drawingCalculationData;
    private DiagramMode _diagramMode;
    private Every _groupInterval;

    #region Drawing implementation

    public DrawingImage Draw(DiagramSeriesUnited diagramData, DiagramDataExtremums diagramDataExtremums,
      DrawingCalculationData drawingCalculationData, DiagramMode diagramMode, Every groupInterval)
    {
      _diagramData = diagramData;
      _extremums = diagramDataExtremums;
      _drawingCalculationData = drawingCalculationData;
      _diagramMode = diagramMode;
      _groupInterval = groupInterval;

      var drawingGroup = new DrawingGroup();

      drawingGroup.Children.Add(FullDiagramBackground(_drawingCalculationData));
      drawingGroup.Children.Add(DiagramRegionBackground(_drawingCalculationData));

      drawingGroup.Children.Add(HorizontalAxes(_drawingCalculationData));
      drawingGroup.Children.Add(VerticalGridLines(_drawingCalculationData));

      drawingGroup.Children.Add(XAxisDashes(Dock.Top, _drawingCalculationData));
      drawingGroup.Children.Add(XAxisDashes(Dock.Bottom, _drawingCalculationData));
      drawingGroup.Children.Add(XAxisMarkers(Dock.Top, _drawingCalculationData));
      drawingGroup.Children.Add(XAxisMarkers(Dock.Bottom, _drawingCalculationData));

      drawingGroup.Children.Add(VerticalAxes(_drawingCalculationData));

      drawingGroup.Children.Add(YAxisDashesWithMarkers(Dock.Left, _drawingCalculationData));
      drawingGroup.Children.Add(YAxisDashesWithMarkers(Dock.Right, _drawingCalculationData));
      drawingGroup.Children.Add(HorizontalGridLines(_drawingCalculationData));

      if (_diagramMode == DiagramMode.BarVertical) BarVerticalDiagram(_drawingCalculationData, ref drawingGroup);
      if (_diagramMode == DiagramMode.Line)
        for (var j = 0; j < _diagramData.SeriesCount; j++) drawingGroup.Children.Add(OneSeriesLine(_drawingCalculationData, j));

      return new DrawingImage(drawingGroup);
    }

    private GeometryDrawing FullDiagramBackground(DrawingCalculationData cd)
    {
      var rectGeometry = new RectangleGeometry { Rect = new Rect(0, 0, cd.ImageWidth, cd.ImageHeight) };
      return new GeometryDrawing { Geometry = rectGeometry, Brush = Brushes.LightYellow };
    }

    private GeometryDrawing DiagramRegionBackground(DrawingCalculationData cd)
    {
      var rectGeometry = new RectangleGeometry
      {
        Rect = new Rect(cd.LeftMargin, cd.TopMargin, cd.ImageWidth - cd.LeftMargin - cd.RightMargin,
                        cd.ImageHeight - cd.TopMargin - cd.BottomMargin)
      };
      return new GeometryDrawing { Geometry = rectGeometry, Brush = Brushes.White };
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

      for (var i = 0; i < _diagramData.DiagramData.Count; i = i + cd.MarkedDash)
      {
        geometryGroupGridlines.Children.Add(
          new LineGeometry(new Point(cd.LeftMargin + cd.Shift / 2 + cd.PointPerDate * (i + 0.5), cd.TopMargin + 5),
                           new Point(cd.LeftMargin + cd.Shift / 2 + cd.PointPerDate * (i + 0.5),
                                     cd.ImageHeight - cd.BottomMargin - 5)));
      }

      return new GeometryDrawing { Geometry = geometryGroupGridlines, Pen = new Pen(Brushes.LightGray, 1) };
    }

    private GeometryDrawing XAxisDashes(Dock flag, DrawingCalculationData cd)
    {
      var geometryGroupDashesAndMarks = new GeometryGroup();

      for (var i = 0; i < _diagramData.DiagramData.Count; i = i + cd.Dash)
      {
        var dashY = flag == Dock.Bottom ? cd.ImageHeight - cd.BottomMargin : cd.TopMargin;
        var dashSize = (i % cd.MarkedDash) == 0 ? 5 : 2;
        var dashGeometry = new LineGeometry(new Point(cd.LeftMargin + cd.Shift / 2 + cd.PointPerDate * (i + 0.5), dashY - dashSize),
                                    new Point(cd.LeftMargin + cd.Shift / 2 + cd.PointPerDate * (i + 0.5), dashY + dashSize));
        geometryGroupDashesAndMarks.Children.Add(dashGeometry);
      }

      return new GeometryDrawing { Geometry = geometryGroupDashesAndMarks, Pen = new Pen(Brushes.Black, 1) };
    }

    private GeometryDrawing XAxisMarkers(Dock flag, DrawingCalculationData cd)
    {
      var geometryGroupDashesAndMarks = new GeometryGroup();

      for (var i = 0; i < _diagramData.DiagramData.Count; i = i + cd.MarkedDash)
      {
        var markY = flag == Dock.Bottom ? cd.ImageHeight : cd.TopMargin;
        var mark = String.Format(GetMarkTemplate(), _diagramData.DiagramData.ElementAt(i).Key);
        var formattedText = new FormattedText(mark, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                              new Typeface("Times New Roman"), 12, Brushes.Black);
        var geometry =
          formattedText.BuildGeometry(new Point(cd.LeftMargin + cd.Shift / 2 + cd.PointPerDate * (i + 0.5) - 18, markY - 20));
        geometryGroupDashesAndMarks.Children.Add(geometry);
      }

      return new GeometryDrawing { Geometry = geometryGroupDashesAndMarks, Pen = new Pen(Brushes.Black, 1) };
    }

    private string GetMarkTemplate()
    {
      switch (_groupInterval)
      {
        case Every.Year: return "{0:yyyy} ";
        case Every.Month: return "{0:M/yyyy} ";
        case Every.Day: return "{0:d/M/yyyy} ";
        default: return "{0 } ";
      }
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

      double pointPerOneValueBefore = _extremums.MaxValue.Equals(_extremums.MinValue) ?
        0 : (cd.ImageHeight - cd.TopMargin - cd.BottomMargin) / (_extremums.MaxValue - _extremums.MinValue);
      double valuesPerDivision = (minPointBetweenDivision > pointPerOneValueBefore) ?
        Math.Ceiling(minPointBetweenDivision / pointPerOneValueBefore) : minPointBetweenDivision / pointPerOneValueBefore;
      double zeros = Math.Floor(Math.Log10(valuesPerDivision));
      cd.AccurateValuesPerDivision = Math.Ceiling(valuesPerDivision / Math.Pow(10, zeros)) * Math.Pow(10, zeros);
      cd.FromDivision = Convert.ToInt32(Math.Floor(_extremums.MinValue / cd.AccurateValuesPerDivision));

      var temp = Convert.ToInt32(Math.Ceiling((_extremums.MaxValue - _extremums.MinValue) / cd.AccurateValuesPerDivision));
      if ((cd.FromDivision + temp) * cd.AccurateValuesPerDivision < _extremums.MaxValue) temp++;
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

    private GeometryDrawing OneSeriesLine(DrawingCalculationData cd, int seriesNumber)
    {
      var allDays = (_extremums.MaxDate - _extremums.MinDate).Days;
      var pointsPerDay = (cd.ImageWidth - cd.LeftMargin - cd.RightMargin - cd.Shift - cd.Gap) / allDays;

      var firstPointX = cd.LeftMargin + cd.Shift / 2 + cd.Gap / 2;

      var geometryGroup = new GeometryGroup();

      for (int i = 0; i < _diagramData.DiagramData.Count - 1; i++)
      {
        var line = new LineGeometry(
          new Point((_diagramData.DiagramData.ElementAt(i).Key - _extremums.MinDate).Days * pointsPerDay + firstPointX,
                    cd.ImageHeight - cd.BottomMargin -
                    (_diagramData.DiagramData.ElementAt(i).Value[seriesNumber] - cd.LowestScaleValue) *
                    cd.PointPerOneValueAfter),
          new Point((_diagramData.DiagramData.ElementAt(i + 1).Key - _extremums.MinDate).Days * pointsPerDay + firstPointX,
                    cd.ImageHeight - cd.BottomMargin -
                    (_diagramData.DiagramData.ElementAt(i + 1).Value[seriesNumber] - cd.LowestScaleValue) *
                    cd.PointPerOneValueAfter));
        geometryGroup.Children.Add(line);
      }

      return new GeometryDrawing { Geometry = geometryGroup, Pen = new Pen(_diagramData.PositiveBrushes.ElementAt(seriesNumber), 2) };
    }

    private void BarVerticalDiagram(DrawingCalculationData cd, ref DrawingGroup drawingGroup)
    {
      var positiveGeometryGroups = new List<GeometryGroup>();
      var negativeGeometryGroups = new List<GeometryGroup>();
      for (var i = 0; i < _diagramData.SeriesCount; i++)
      {
        positiveGeometryGroups.Add(new GeometryGroup());
        negativeGeometryGroups.Add(new GeometryGroup());
      }

      for (var i = 0; i < _diagramData.DiagramData.Count; i++)
      {
        var oneDay = _diagramData.DiagramData.ElementAt(i).Value;
        double hasAlreadyPlus = 0;
        double hasAlreadyMinus = 0;
        for (var j = 0; j < _diagramData.SeriesCount; j++)
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

      for (var i = 0; i < _diagramData.SeriesCount; i++)
      {
        var positiveGeometryDrawing = new GeometryDrawing
        {
          Geometry = positiveGeometryGroups[i],
          Brush = _diagramData.PositiveBrushes.ElementAt(i),
          Pen = new Pen(_diagramData.PositiveBrushes.ElementAt(i), 1)
        };

        drawingGroup.Children.Add(positiveGeometryDrawing);

        var negativeGeometryDrawing = new GeometryDrawing
        {
          Geometry = negativeGeometryGroups[i],
          Brush = _diagramData.NegativeBrushes.ElementAt(i),
          Pen = new Pen(_diagramData.NegativeBrushes.ElementAt(i), 1)
        };

        drawingGroup.Children.Add(negativeGeometryDrawing);
      }
    }

    #endregion


  }
}
