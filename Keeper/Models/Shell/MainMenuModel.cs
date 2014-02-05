using Caliburn.Micro;

namespace Keeper.Models.Shell
{
  public enum Actions
  {
    Idle,

    InputTransactions,
    InputRates,
    InputAssociates,
    ShowAnalisys,

    LoadDatabase,
    LoadFromFiles,
    CleanDatabase,
    SaveDatabase,
    RemoveIdenticalBackups,
    PrepareExit,

    RefreshBalanceList
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
