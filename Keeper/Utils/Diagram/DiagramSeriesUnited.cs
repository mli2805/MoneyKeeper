using System;
using System.Collections.Generic;

namespace Keeper.Utils
{
  public class DiagramSeriesUnited
  {
    public SortedList<DateTime, List<double>> DiagramData;
    public int SeriesCount;

    public DiagramSeriesUnited()
    {
      SeriesCount = 0;
      DiagramData = new SortedList<DateTime, List<double>>();
    }

    public DiagramSeriesUnited(DiagramSeriesUnited other)
    {
      SeriesCount = other.SeriesCount;
      DiagramData = new SortedList<DateTime, List<double>>(other.DiagramData);
    }

    public void Add(DiagramSeries series)
    {
      foreach (var pair in series.Data)
      {
        if (!DiagramData.ContainsKey(pair.CoorXdate)) DiagramData.Add(pair.CoorXdate,new List<double>());
        while (DiagramData[pair.CoorXdate].Count < SeriesCount) DiagramData[pair.CoorXdate].Add(0);
        DiagramData[pair.CoorXdate].Add(pair.CoorYdouble);
      }
      SeriesCount++;
    }
  }
}