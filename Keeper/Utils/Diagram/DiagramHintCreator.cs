using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Keeper.Utils.Diagram
{
  class DiagramHintCreator
  {
    private readonly DiagramData _allDiagramData;
    private readonly SortedList<DateTime, List<double>> _currentSeriesUnitedData;
    private readonly Every _groupInterval;
    private readonly DiagramMode _diagramMode;
    private readonly DiagramDrawingCalculator _diagramDrawingCalculator;

    public DiagramHintCreator(DiagramData allDiagramData, SortedList<DateTime, List<double>> currentSeriesUnitedData,
                    Every groupInterval, DiagramMode diagramMode, DiagramDrawingCalculator diagramDrawingCalculator)
    {
      _allDiagramData = allDiagramData;
      _currentSeriesUnitedData = currentSeriesUnitedData;
      _groupInterval = groupInterval;
      _diagramMode = diagramMode;
      _diagramDrawingCalculator = diagramDrawingCalculator;
    }

    private Brush DefineBackground(int barNumber)
    {
      if (_allDiagramData.Data.Count == 1) return _currentSeriesUnitedData.ElementAt(barNumber).Value[0] > 0 ?
                                                                       Brushes.Azure : Brushes.LavenderBlush;
      return Brushes.White;
    }

    private string DefineContent(int barNumber)
    {
      var thisBar = _currentSeriesUnitedData.ElementAt(barNumber);
      var content = _groupInterval == Every.Month
                          ? "  {0:MMMM yyyy}  "
                          : "  {0:yyyy} год  ";

      if (_allDiagramData.Data.Count == 1)
      {
        content += "\n  {1:0} usd ";
        return string.Format(content, thisBar.Key, thisBar.Value[0]);
      }

      var i = 0;
      content = string.Format(content, thisBar.Key);
      foreach (var series in _allDiagramData.Data)
      {
        if (!thisBar.Value[i].Equals(0))
          content += string.Format("\n  {0} = {1:0} usd  ", series.Name, thisBar.Value[i]);
        i++;
      }

      return content;
    }

    public bool CreateHint(Point pt, out string context, out Brush background)
    {
      context = "";
      background = Brushes.White;
      switch (_diagramMode)
      {
        case DiagramMode.BarVertical:
          int barLeft;
          bool isOverBar;
          var pointAnalyzer = new DiagramPointAnalyzer(_currentSeriesUnitedData, _diagramDrawingCalculator);
          var bar = pointAnalyzer.PointToBar(pt, out barLeft, out isOverBar);
          if (isOverBar)
          {
            background = DefineBackground(bar);
            context = DefineContent(bar);
            return true;
          }

          if (bar != -1)
            Console.WriteLine("  {0}:{1}   Mouse pointer missed {2}th bar by height", pt.X, pt.Y, bar + 1);
          else
            Console.WriteLine("  {0}:{1}   Mouse pointer is to the right of {2}th bar", pt.X, pt.Y, barLeft + 1);
          return false;
        case DiagramMode.Lines:
        case DiagramMode.SeparateLines:
          return false;
        default:
          return false;
      }
    }

  }
}
