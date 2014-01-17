using Caliburn.Micro;

namespace Keeper.Models
{
  public enum Actions
  {
    Idle,
    DatabaseLoaded,
    DatabaseCleaned,
    SaveWithProgressBar,
    PreparingExit,
    Quit
  }

  public class MainMenuModel : PropertyChangedBase
  {
    private Actions _action;
    public Actions Action
    {
      get { return _action; }
      set
      {
        if (value == _action) return;
        _action = value;
        NotifyOfPropertyChange(() => Action);
      }
    }
  }
}
