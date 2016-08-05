using System;
using System.Linq;
using System.Windows;
using Keeper.Controls;
using Keeper.DomainModel.Enumes;

namespace Keeper.Utils.DiagramMy
{
  public class DiagramDrawingCalculator
  {
    public DiagramDrawingCalculator(BarDiagramControl owner)
    {
      Owner = owner;
    }

    private BarDiagramControl Owner { get; set; }

    public double ImageWidth { get { return Owner.IsLoaded ? Owner.ActualWidth : SystemParameters.FullPrimaryScreenWidth; } }
    public double ImageHeight { get { return Owner.IsLoaded ? (Owner.ActualHeight - Owner.StatusBar.ActualHeight) : SystemParameters.FullPrimaryScreenHeight; } }

    public double LeftMargin { get { return ImageWidth * 0.04; } }
    public double RightMargin { get { return ImageWidth * 0.04; } }
    public double TopMargin { get { return ImageHeight * 0.03; } }
    public double BottomMargin { get { return ImageHeight * 0.03; } }

    // расчет горизонтальной оси
    public double MinPointBetweenDivision { get { return 10; }}
    public double MinPointBetweenMarkedDivision
    {
      get
      {
        switch (Owner.GroupInterval)
        {
          case Every.Year: return 30;
          case Every.Month: return 50;
          case Every.Day: return 80;
          default: return 100;
        }
      }
    }

    public double Shift { get { return (int)Owner.DiagramMode > 100 ? 7 : ImageWidth * 0.002; } } // от левой оси до первого столбика (начала линии)
    public double PointPerDataElement
    {
      get
      {
        if (Owner.CurrentSeriesUnited.DiagramData == null) return 0;
        if (Owner.CurrentSeriesUnited.DiagramData.Count == 0) return 0;
        return (ImageWidth - LeftMargin - RightMargin - Shift) / Owner.CurrentSeriesUnited.DiagramData.Count;
      }
    }

    public double Gap { get { return PointPerDataElement / 3; } } // промежуток между столбиками диаграммы
    public double PointPerBar { get { return PointPerDataElement - Gap; } }

    public int Dash { get { return (int)Math.Ceiling(MinPointBetweenDivision / PointPerDataElement); } }
    public int MarkedDash
    {
      get
      {
        var temp = Math.Ceiling(MinPointBetweenMarkedDivision / PointPerDataElement);
        return Dash * (int)Math.Round(temp / Dash);
      }
    }

    public double PointPerDay
    {
      get
      {
        if (Owner.CurrentSeriesUnited.DiagramData == null) return 0;
        if (Owner.CurrentSeriesUnited.DiagramData.Count == 0) return 0;
        return (ImageWidth - LeftMargin - RightMargin - Shift)/
               (Owner.CurrentSeriesUnited.DiagramData.Last().Key - Owner.CurrentSeriesUnited.DiagramData.ElementAt(0).Key).Days;
      }
    }

    //расчет вертикальной оси
    public double MinPointBetweenVertDivision { get { return 35; } }
    public double PointPerOneValueBefore
    {
      get
      {
        return Owner.DiagramDataExtremums.MaxValue.Equals(Owner.DiagramDataExtremums.MinValue) ? 
          0 : (ImageHeight - TopMargin - BottomMargin) / (Owner.DiagramDataExtremums.MaxValue - Owner.DiagramDataExtremums.MinValue);
      }
    }
    public double ValuesPerDivision
    {
      get
      {
        return (MinPointBetweenVertDivision > PointPerOneValueBefore)
                 ? Math.Ceiling(MinPointBetweenVertDivision/PointPerOneValueBefore)
                 : MinPointBetweenVertDivision/PointPerOneValueBefore;
      }
    }
    public double Zeros { get { return Math.Floor(Math.Log10(ValuesPerDivision)); } }
    public double AccurateValuesPerDivision { get { return Math.Ceiling(ValuesPerDivision / Math.Pow(10, Zeros)) * Math.Pow(10, Zeros); } }
    public int FromDivision { get { return Convert.ToInt32(Math.Floor(Owner.DiagramDataExtremums.MinValue / AccurateValuesPerDivision)); } }

    public int Divisions
    {
      get
      {
        var temp = Convert.ToInt32(Math.Ceiling((Owner.DiagramDataExtremums.MaxValue - Owner.DiagramDataExtremums.MinValue) / AccurateValuesPerDivision));
        if ((FromDivision + temp) * AccurateValuesPerDivision < Owner.DiagramDataExtremums.MaxValue) temp++;
        return temp;
      }
    }

    public double PointPerScaleStep { get { return (ImageHeight - TopMargin - BottomMargin) / Divisions; } }

    public double PointPerOneValueAfter { get { return (ImageHeight - TopMargin - BottomMargin) / (Divisions * AccurateValuesPerDivision); } }
    public double Y0
    {
      get
      {
        var temp = ImageHeight - BottomMargin;
        if (FromDivision < 0) temp += PointPerScaleStep * FromDivision;
        return temp;
      }
    }
    public double LowestScaleValue { get { return FromDivision * AccurateValuesPerDivision; } }
  }

}
