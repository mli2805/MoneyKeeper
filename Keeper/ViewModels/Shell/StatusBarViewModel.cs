using System;
using System.Composition;
using System.Windows;
using Caliburn.Micro;
using Keeper.Models.Shell;

namespace Keeper.ViewModels.Shell
{
  [Export]
  public class StatusBarViewModel : Screen
  {
    public StatusBarModel MyStatusBarModel { get; set; }

    [ImportingConstructor]
    public StatusBarViewModel(ShellModel shellModel)
    {
      MyStatusBarModel = shellModel.MyStatusBarModel;
    }

    protected override void OnViewLoaded(object view)
    {
      MyStatusBarModel.Item0 = "Idle";
      MyStatusBarModel.ProgressBarVisibility = Visibility.Collapsed;
      MyStatusBarModel.Message = DateTime.Today.ToString("dddd , dd MMMM yyyy");

    }
  }
}
