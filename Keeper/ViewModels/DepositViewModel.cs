using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils;

namespace Keeper.ViewModels
{
  class DepositViewModel : Screen
  {
    public Deposit Deposit { get; set; }

    public DepositViewModel(Account account)
    {
      Deposit = new Deposit { Account = account };
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = Deposit.Account.Name;
      Deposit.MakeReport();
    }

  }
}
