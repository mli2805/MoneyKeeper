using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  class DepositsViewModel : Screen
  {
    public static KeeperTxtDb Db { get { return IoC.Get<KeeperTxtDb>(); } }
    public List<Deposit> DepositsList { get; set; }
    public List<string> TotalsList { get; set; } 

    public DepositsViewModel()
    {
      DepositsList = new List<Deposit>();
      foreach (var account in Db.AccountsPlaneList)
      {
        if (account.IsDescendantOf("Депозиты") && account.Children.Count == 0)
        {
          var temp = new Deposit {Account = account};
          temp.CollectInfo();
          DepositsList.Add(temp);
        }
      }

      TotalsList = new List<string>{"Итоговые показатели:"};
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = "Депозиты";
    }
  }
}
