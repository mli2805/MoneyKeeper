using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  class DepositRatesViewModel : Screen
  {
    public ObservableCollection<DepositRateLine> Rows { get; set; }

    public DepositRatesViewModel(Deposit deposit)
    {
      Rows = deposit.DepositRateLines;
      if (Rows == null)
      {
        Rows = new ObservableCollection<DepositRateLine>();
        Rows.Add(new DepositRateLine{AmountFrom = 0, AmountTo = Decimal.MaxValue, DateFrom = deposit.StartDate, DateTo = deposit.FinishDate, Rate = deposit.DepositRate});
      }
    }
  }
}
