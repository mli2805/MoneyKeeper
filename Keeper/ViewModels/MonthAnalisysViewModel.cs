using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils;

namespace Keeper.ViewModels
{
  class MonthAnalisysViewModel : Screen
  {
    public static KeeperTxtDb Db { get {return IoC.Get<KeeperTxtDb>(); }
    }

    public List<String> MainList { get; set; }

    public MonthAnalisysViewModel()
    {
      MainList = new List<string>();
      Calculate();
    }

    private void Calculate()
    {
      CalculateStartBalance();
      CalculateIncomes();
      CalculateExpense();
      CalculateEndBalance();
    }

    private void CalculateStartBalance()
    {
      var myAccountsRoot = (from account in Db.Accounts
                            where account.Name == "Мои"
                            select account).FirstOrDefault();

      var startDate = new DateTime(2013, 2, 1);
      var startBalance = Balance.AccountBalancePairsBeforeDay(myAccountsRoot, startDate);
      decimal balanceInUsd = 0;
      foreach (var balancePair in startBalance)
      {
        if (balancePair.Amount == 0) continue;
        if (balancePair.Currency != CurrencyCodes.USD)
        {
          decimal amountInUsd;
          Rate.GetUsdEquivalent(balancePair.Amount, (CurrencyCodes)balancePair.Currency, startDate, out amountInUsd);
          balanceInUsd += amountInUsd;
          MainList.Add(balancePair.ToString() + "  (= " + amountInUsd.ToString("F0") + " $)");
        }
        else
        {
          balanceInUsd += balancePair.Amount;
          MainList.Add(balancePair.ToString());
        }
      }
      MainList.Add(String.Format("Итого {0:#,0} usd",balanceInUsd));

    }

    private void CalculateIncomes()
    {

    }

    private void CalculateExpense()
    {

    }

    private void CalculateEndBalance()
    {

    }


  }
}
