using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  class DepositViewModel : Screen
  {
    private readonly Deposit _deposit;
    public List<string> DepositReport { get; set; }

    public DepositViewModel(Account account)
    {
      _deposit = new Deposit {Account = account};
      DepositReport = new List<string>();
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = _deposit.Account.Name;
      _deposit.CollectInfo();
      MakeReport();
    }

    private void MakeReport()
    {
      if (_deposit.Finish < DateTime.Today) DepositReport.Add("!!! Срок депозита истек !!!");

      foreach (var transaction in _deposit.Transactions)
      {
        DepositReport.Add(transaction.ToDepositReport(_deposit.Account));
      }
    }

  
  }
}
