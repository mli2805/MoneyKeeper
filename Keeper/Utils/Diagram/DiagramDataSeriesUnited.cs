using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Keeper.Utils.Diagram;

namespace Keeper.Utils
{
  public class DiagramDataExtremums
  {
    public DateTime MinDate, MaxDate;
    public double MinValue, MaxValue;
  }

  public class DiagramDataSeriesUnited
  {
    public SortedList<DateTime, List<double>> DiagramData;
    public List<Brush> PositiveBrushes;
    public List<Brush> NegativeBrushes;
    public List<string> Names;
    public int SeriesCount;

    public DiagramDataSeriesUnited()
    {
      SeriesCount = 0;
      DiagramData = new SortedList<DateTime, List<double>>();
    }

    public DiagramDataSeriesUnited(DiagramData allSeries)
    {
      DiagramData = new SortedList<DateTime, List<double>>();
      PositiveBrushes = new List<Brush>();
      NegativeBrushes = new List<Brush>();
      Names = new List<string>();

      foreach (var series in allSeries.Data)
      {
        Names.Add(series.Name);
        foreach (var pair in series.Data)
        {
          if (!DiagramData.ContainsKey(pair.CoorXdate)) DiagramData.Add(pair.CoorXdate, new List<double>());
          while (DiagramData[pair.CoorXdate].Count < SeriesCount) DiagramData[pair.CoorXdate].Add(0);
          DiagramData[pair.CoorXdate].Add(pair.CoorYdouble);
        }
        PositiveBrushes.Add(series.PositiveBrushColor);
        NegativeBrushes.Add(series.NegativeBrushColor);

      }
      SeriesCount = allSeries.Data.Count;
    }

    public DiagramDataSeriesUnited(DiagramDataSeriesUnited other)
    {
      DiagramData = new SortedList<DateTime, List<double>>(other.DiagramData);
      PositiveBrushes = new List<Brush>(other.PositiveBrushes);
      NegativeBrushes = new List<Brush>(other.NegativeBrushes);
      Names = new List<string>(other.Names);
      SeriesCount = other.SeriesCount;
    }

    public DiagramDataExtremums FindDataExtremums(DiagramMode diagramMode)
    {
      var dataExtremums = new DiagramDataExtremums();

      dataExtremums.MinDate = DiagramData.ElementAt(0).Key;
      dataExtremums.MaxDate = DiagramData.Last().Key;

      switch (diagramMode)
      {
        case DiagramMode.Line:
        case DiagramMode.BarHorizontal:
          // это вариант , когда столбцы разных серий стоят рядом
          dataExtremums.MinValue = DiagramData.Values.Min(l => l.Min());
          dataExtremums.MaxValue = DiagramData.Values.Max(l => l.Max());
          break;

        case DiagramMode.BarVertical:
          // ряд серий отрицательные, либо даже просто значение отрицательное в положительной серии
          dataExtremums.MinValue = dataExtremums.MaxValue = 0;
          foreach (var day in DiagramData)
          {
            var plus = day.Value.Where(values => values > 0).Sum();
            var minus = day.Value.Where(values => values < 0).Sum();
            if (plus > dataExtremums.MaxValue) dataExtremums.MaxValue = plus;
            if (minus < dataExtremums.MinValue) dataExtremums.MinValue = minus;
          }
          break;
      }

      if (dataExtremums.MaxValue > 0 && dataExtremums.MinValue > 0) dataExtremums.MinValue = 0;
      if (dataExtremums.MaxValue < 0 && dataExtremums.MinValue < 0) dataExtremums.MaxValue = 0;

      return dataExtremums;
    }

  }
}