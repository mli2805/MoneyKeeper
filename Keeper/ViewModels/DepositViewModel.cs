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
    private readonly Deposit _deposit;
    public List<string> DepositReport { get; set; }

    public DepositViewModel(Account account)
    {
      _deposit = new Deposit { Account = account };
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
      decimal balance = Balance.GetBalanceInCurrency(_deposit.Account,
                                                     new Period(new DateTime(1001, 1, 1), DateTime.Today),
                                                     _deposit.MainCurrency);
      if (_deposit.Finish < DateTime.Today)
        DepositReport.Add(balance == 0 ? "Депозит закрыт. Остаток 0.\n" : "!!! Срок депозита истек !!!\n");
      else
      {
        var balanceString = _deposit.MainCurrency != CurrencyCodes.USD ?
          String.Format("{0:#,0} byr  ($ {1:#,0} )", balance, balance / (decimal)Rate.GetRate(_deposit.MainCurrency, DateTime.Today.Date)) :
          String.Format("{0:#,0} usd", balance);
        DepositReport.Add(String.Format("Действующий депозит. Остаток на {0:dd/MM/yyyy} составляет {1} \n", DateTime.Today, balanceString));
      }
      DepositReport.Add("                             Расход                          Доход ");
      var incomeComment = "открытие счета";
      foreach (var transaction in _deposit.Transactions)
      {
        var comment = transaction.Credit == _deposit.Account ? incomeComment : "";
        DepositReport.Add(String.Format("{0}     {1}",transaction.ToDepositReport(_deposit.Account),comment));
        incomeComment = "доп взнос или начисление процентов";
      }

      DepositReport.Add("");
      DepositReport.Add(String.Format("Доход по депозиту {0:#,0} usd \n",_deposit.Profit));
      DepositReport.Add("");
    }


  }
}
