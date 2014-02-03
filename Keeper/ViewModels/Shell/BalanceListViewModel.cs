using System.Composition;
using Caliburn.Micro;
using Keeper.Models;
using Keeper.Models.Shell;

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
