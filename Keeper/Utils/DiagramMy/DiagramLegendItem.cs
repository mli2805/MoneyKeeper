using System.Windows.Media;
using Caliburn.Micro;

namespace Keeper.Utils.DiagramMy
{
  public class DiagramLegendItem : PropertyChangedBase
  {
    private string _seriesName;
    private Brush _fontColor;

    public string SeriesName
    {
      get { return _seriesName; }
      set
      {
        if (value == _seriesName) return;
        _seriesName = value;
        NotifyOfPropertyChange(() => SeriesName);
      }
    }

    public Brush FontColor
    {
      get { return _fontColor; }
      set
      {
        if (Equals(value, _fontColor)) return;
        _fontColor = value;
        NotifyOfPropertyChange(() => FontColor);
      }
    }
  }
}