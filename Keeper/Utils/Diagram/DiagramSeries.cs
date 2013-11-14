using System.Collections.Generic;
using System.Windows.Media;

namespace Keeper.Utils
{
  public class DiagramSeries
  {
    public string Name;
    public Brush positiveBrushColor;
    public Brush negativeBrushColor;
    public int Index;
    public List<DiagramPair> Data;
  }
}