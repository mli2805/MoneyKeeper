using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  public class OpenOrEditDepositViewModel : Screen
  {
    public Deposit Deposit { get; set; }
    private readonly string _windowTitle; 

    public OpenOrEditDepositViewModel(Deposit deposit, string windowTitle)
    {
      Deposit = deposit;
      _windowTitle = windowTitle;
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = _windowTitle;
    }
  }
}
