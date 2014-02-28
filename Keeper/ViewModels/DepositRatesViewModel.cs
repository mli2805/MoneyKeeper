using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  class DepositRatesViewModel : Screen
  {
    public DateTime NewDate { get; set; }

    public ObservableCollection<DepositRateLine> Rows { get; set; }

    public DepositRatesViewModel(Deposit deposit)
    {
      NewDate = DateTime.Today;
      if (deposit.DepositRateLines == null) deposit.DepositRateLines = new ObservableCollection<DepositRateLine>();
      Rows = deposit.DepositRateLines;
      if (Rows.Count == 0)
        Rows.Add(new DepositRateLine { DateFrom = deposit.StartDate, DateTo = deposit.FinishDate, AmountFrom = 0, AmountTo = 999999999999, Rate = deposit.DepositRate });
    }

    public void AddLine()
    {
      if (Rows.Count == 0) return;
      var lastLine = Rows[Rows.Count - 1];
      var newLine = new DepositRateLine
                      {
                        DateFrom = lastLine.DateFrom,
                        AmountFrom = lastLine.AmountTo + 1,
                        AmountTo = 999999999999,
                        Rate = lastLine.Rate + 1
                      };
      Rows.Add(newLine);
    }

    public void RepeatDay()
    {
      if (Rows.Count == 0) return;
      var lastDate = Rows[Rows.Count - 1].DateFrom;

      var lastDateLines = Rows.Where(depositRateLine => depositRateLine.DateFrom == lastDate).ToList();

      foreach (var line in lastDateLines)
      {
        Rows.Add(new DepositRateLine{DateFrom = NewDate, AmountFrom = line.AmountFrom, AmountTo = line.AmountTo, Rate = line.Rate});
      }
    }

    public void Close(){TryClose(true);}


  }
}
