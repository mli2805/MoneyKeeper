using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Keeper.Utils.Diagram
{
  public enum DiagramMode
  {
    BarHorizontal, // столбцы разных серий для одной даты находятся рядом, могут быть отрицательные
    BarVertical, // столбцы для одной даты ставятся один на один, могут быть отрицательные
    BarVertical100, // столбцы для одной даты ставятся один на один и сумма считается за 100%, не должно быть отрицательных
    Line
  }
  public class DiagramData
  {
    public string Caption;
    public List<DiagramSeries> Data;
    public DiagramMode Mode;
    public Every TimeInterval;
  }

  public class DiagramSeries
  {
    public string Name;
    public Brush PositiveBrushColor;
    public Brush NegativeBrushColor;
    public int Index;
    public List<DiagramPair> Data;
  }

  public class DiagramPair
  {
    public DateTime CoorXdate;
    public double CoorYdouble;

    public DiagramPair(DateTime coorXdate, double coorYdouble)
    {
      CoorXdate = coorXdate;
      CoorYdouble = coorYdouble;
    }
  }

}
