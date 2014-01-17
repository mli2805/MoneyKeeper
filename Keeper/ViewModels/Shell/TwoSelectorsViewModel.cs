using System.Composition;
using Caliburn.Micro;
using Keeper.Models;
using Keeper.Models.ShellModels;

namespace Keeper.ViewModels.Shell
{
  [Export]
  public class TwoSelectorsViewModel : Screen
  {
    public TwoSelectorsModel MyTwoSelectorsModel { get; set; }

    [ImportingConstructor]
    public TwoSelectorsViewModel(ShellModel shellModel)
    {
      MyTwoSelectorsModel = shellModel.MyTwoSelectorsModel;
    }
  }
}
