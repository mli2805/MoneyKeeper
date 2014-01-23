using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace Keeper.Models.ShellModels
{
  public class BalanceListModel : PropertyChangedBase
  {
    private ObservableCollection<string> _balanceList;
    private string _accountBalanceInUsd;
    private string _caption;

    public BalanceListModel()
    {
      _balanceList = new ObservableCollection<string>();
    }

    public ObservableCollection<string> BalanceList
    {
      get { return _balanceList; }
      set
      {
        if (Equals(value, _balanceList)) return;
        _balanceList = value;
        NotifyOfPropertyChange(() => BalanceList);
      }
    }

    public string AccountBalanceInUsd
    {
      get { return _accountBalanceInUsd; }
      set
      {
        if (value == _accountBalanceInUsd) return;
        _accountBalanceInUsd = value;
        NotifyOfPropertyChange(() => AccountBalanceInUsd);
      }
    }

    public string Caption
    {
      get { return _caption; }
      set
      {
        if (value == _caption) return;
        _caption = value;
        NotifyOfPropertyChange(() => Caption);
      }
    }

  }
}