using System.Collections.Generic;
using System.Composition;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Models;

namespace Keeper.ViewModels.Shell
{
  [Export]
  public class BalanceListViewModel : Screen
  {
    public BalanceListModel MyBalanceListModel { get; set; }

    [ImportingConstructor]
    public BalanceListViewModel(ShellModel shellModel)
    {
      MyBalanceListModel = shellModel.MyBalanceListModel;
    }
  }
}
