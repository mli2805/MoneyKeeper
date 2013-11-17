using System.Windows;
using Keeper.Controls;

namespace Keeper.Utils.Diagram
{
  public class DrawingCalculationData
  {
    public DrawingCalculationData(BarDiagramControl owner)
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
    public double Shift { get { return ImageWidth * 0.002; } } // от левой оси до первого столбика
    public double PointPerDate
    {
      get
      {
        if (Owner.CurrentSeriesUnited.DiagramData == null) return 0;
        if (Owner.CurrentSeriesUnited.DiagramData.Count == 0) return 0;
        return (ImageWidth - LeftMargin - RightMargin - Shift) / Owner.CurrentSeriesUnited.DiagramData.Count;
      }
    }
    public double Gap { get { return PointPerDate / 3; } } // промежуток между столбиками диаграммы
    public double PointPerBar { get { return PointPerDate - Gap; } }

    //расчет вертикальной оси
    public double AccurateValuesPerDivision;
    public int FromDivision;
    public int Divisions;
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
