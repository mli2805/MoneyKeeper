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
    class BankDepositRatesAndRulesViewModel : Screen
    {
        private readonly KeeperDb _db;
        private Deposit _depositInWork;
        public DateTime NewDate { get; set; }
        public List<Account> DepositsForCombobox { get; set; }
        public Account DepositDonor { get; set; }

        public Deposit DepositInWork
        {
            get { return _depositInWork; }
            set
            {
                if (Equals(value, _depositInWork)) return;
                _depositInWork = value;
                NotifyOfPropertyChange(() => DepositInWork);
            }
        }
        public ObservableCollection<DepositRateLine> Rows { get; set; }

        [ImportingConstructor]
        public BankDepositRatesAndRulesViewModel(KeeperDb db, AccountTreeStraightener accountTreeStraightener)
        {
            _db = db;
            NewDate = DateTime.Today;
            DepositsForCombobox = accountTreeStraightener.Flatten(_db.Accounts).Where(a => a.Deposit != null).ToList();
        }

        public void Initialize(Deposit deposit)
        {
            if (deposit.ProcentsCalculatingRules == null) deposit.ProcentsCalculatingRules = new DepositProcentsCalculatingRules();
            DepositInWork = deposit;

            if (deposit.RateLines == null) deposit.RateLines = new ObservableCollection<DepositRateLine>();
            Rows = deposit.RateLines;
            if (Rows.Count == 0)
                Rows.Add(new DepositRateLine { DateFrom = deposit.StartDate, AmountFrom = 0, AmountTo = 999999999999, Rate = 100 });
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Ставки и правила";
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
                Rows.Add(new DepositRateLine { DateFrom = NewDate, AmountFrom = line.AmountFrom, AmountTo = line.AmountTo, Rate = line.Rate });
            }
        }

        public void RepeatFromDeposit()
        {
            if (DepositDonor == null || DepositDonor.Deposit == null || DepositDonor.Deposit.RateLines == null || DepositDonor.Deposit.RateLines.Count == 0) return;
            var otherRows = DepositDonor.Deposit.RateLines;

            var lastDate = otherRows[otherRows.Count - 1].DateFrom;
            var lastDateLines = otherRows.Where(depositRateLine => depositRateLine.DateFrom == lastDate).ToList();

            foreach (var line in lastDateLines)
            {
                Rows.Add(new DepositRateLine { DateFrom = NewDate, AmountFrom = line.AmountFrom, AmountTo = line.AmountTo, Rate = line.Rate });
            }
        }

        public void ImportFrom()
        {
            if (DepositDonor == null || DepositDonor.Deposit == null || DepositDonor.Deposit.RateLines == null || DepositDonor.Deposit.RateLines.Count == 0) return;
            Rows.Clear();
            foreach (var line in DepositDonor.Deposit.RateLines)
            {
                Rows.Add(new DepositRateLine { DateFrom = line.DateFrom, AmountFrom = line.AmountFrom, AmountTo = line.AmountTo, Rate = line.Rate });
            }
            DepositInWork.ProcentsCalculatingRules.EveryFirstDayOfMonth = DepositDonor.Deposit.ProcentsCalculatingRules.EveryFirstDayOfMonth;
            DepositInWork.ProcentsCalculatingRules.EveryLastDayOfMonth  = DepositDonor.Deposit.ProcentsCalculatingRules.EveryLastDayOfMonth ;
            DepositInWork.ProcentsCalculatingRules.EveryStartDay        = DepositDonor.Deposit.ProcentsCalculatingRules.EveryStartDay       ;
            DepositInWork.ProcentsCalculatingRules.IsCapitalized        = DepositDonor.Deposit.ProcentsCalculatingRules.IsCapitalized       ;
            DepositInWork.ProcentsCalculatingRules.IsFactDays           = DepositDonor.Deposit.ProcentsCalculatingRules.IsFactDays          ;
            DepositInWork.ProcentsCalculatingRules.OnlyAtTheEnd         = DepositDonor.Deposit.ProcentsCalculatingRules.OnlyAtTheEnd        ;
            NotifyOfPropertyChange(() => DepositInWork);

        }

        public void CloseForm()
        {
            TryClose(true);
        }


    }
}
