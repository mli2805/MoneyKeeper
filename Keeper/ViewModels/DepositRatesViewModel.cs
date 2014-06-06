using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils.Accounts;

namespace Keeper.ViewModels
{
  [Export]
  class DepositRatesViewModel : Screen
  {
    private readonly KeeperDb _db;
    public DateTime NewDate { get; set; }
    public List<Account> DepositsForCombobox { get; set; }
    public Account DepositDonor { get; set; }

    public ObservableCollection<DepositRateLine> Rows { get; set; }

    [ImportingConstructor]
    public DepositRatesViewModel(KeeperDb db, AccountTreeStraightener accountTreeStraightener)
    {
      _db = db;
      NewDate = DateTime.Today;
      DepositsForCombobox = accountTreeStraightener.Flatten(_db.Accounts).Where(a=>a.Deposit != null).ToList();
    }

    public void Initialize(Deposit deposit)
    {
      if (deposit.DepositRateLines == null) deposit.DepositRateLines = new ObservableCollection<DepositRateLine>();
      Rows = deposit.DepositRateLines;
      if (Rows.Count == 0)
        Rows.Add(new DepositRateLine { DateFrom = deposit.StartDate, AmountFrom = 0, AmountTo = 999999999999, Rate = 100 });
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = "Таблица процентных ставок по вкладу";
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

    public void RepeatFromDeposit()
    {
      if (DepositDonor.Deposit == null || DepositDonor.Deposit.DepositRateLines == null || DepositDonor.Deposit.DepositRateLines.Count == 0) return;
      var otherRows = DepositDonor.Deposit.DepositRateLines;

      var lastDate = otherRows[otherRows.Count - 1].DateFrom;
      var lastDateLines = otherRows.Where(depositRateLine => depositRateLine.DateFrom == lastDate).ToList();

      foreach (var line in lastDateLines)
      {
        Rows.Add(new DepositRateLine { DateFrom = NewDate, AmountFrom = line.AmountFrom, AmountTo = line.AmountTo, Rate = line.Rate });
      }
    }

    public void CloseForm(){TryClose(true);}


  }
}
