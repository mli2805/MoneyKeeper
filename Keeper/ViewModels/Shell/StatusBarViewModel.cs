using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Keeper.Models;

namespace Keeper.ViewModels.Shell
{
  class StatusBarViewModel : Screen
  {
    public StatusBarModel MyStatusBarModel { get; set; }

    public StatusBarViewModel(ShellModel shellModel)
    {
      MyStatusBarModel = shellModel.MyStatusBarModel;
    }
  }
}
