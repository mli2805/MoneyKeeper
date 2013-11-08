using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Keeper.ViewModels
{
  class LogonViewModel : Screen
  {
    [Import]
    public IWindowManager WindowManager { get; set; }

    public LogonViewModel()
    {
      WindowManager.ShowDialog(new ShellViewModel());

    }
  }
}
