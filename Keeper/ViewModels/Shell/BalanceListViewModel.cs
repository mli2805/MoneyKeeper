using System.Collections.Generic;
using System.Composition;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels.Shell
{
  [Export]
  public class BalanceListViewModel : Screen
  {
    public List<string> BalanceList { get; set; }
    public Account SelectedAccount { get; set; }
    public string AccountBalanceInUsd { get; set; }
      
      [ImportingConstructor]
    public BalanceListViewModel()
    {
    }
  }
}
