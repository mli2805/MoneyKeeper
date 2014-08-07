﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.DomainModel.Deposit;
using Keeper.Utils.Accounts;

namespace Keeper.ViewModels
{
    [Export]
    class BankDepositRatesAndRulesViewModel : Screen
    {
        private readonly KeeperDb _db;
        private BankDepositOffer _depositOfferInWork;
        public DateTime NewDate { get; set; }
        public List<Account> DepositsForCombobox { get; set; }
        public BankDepositOffer DepositOfferDonor { get; set; }

        public BankDepositOffer DepositOfferInWork
        {
            get { return _depositOfferInWork; }
            set
            {
                if (Equals(value, _depositOfferInWork)) return;
                _depositOfferInWork = value;
                NotifyOfPropertyChange(() => DepositOfferInWork);
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

        public void Initialize(BankDepositOffer depositOffer)
        {
            if (depositOffer.CalculatingRules == null) depositOffer.CalculatingRules = new BankDepositCalculatingRules();
            DepositOfferInWork = depositOffer;

            if (depositOffer.RateLines == null) depositOffer.RateLines = new ObservableCollection<DepositRateLine>();
            Rows = depositOffer.RateLines;
            if (Rows.Count == 0)
                Rows.Add(new DepositRateLine { DateFrom = DateTime.Today, AmountFrom = 0, AmountTo = 999999999999, Rate = 100 });
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

        public void ImportFrom()
        {
            if (DepositOfferDonor == null ||DepositOfferDonor.RateLines == null || DepositOfferDonor.RateLines.Count == 0) return;
            Rows.Clear();
            foreach (var line in DepositOfferDonor.RateLines)
            {
                Rows.Add(new DepositRateLine { DateFrom = line.DateFrom, AmountFrom = line.AmountFrom, AmountTo = line.AmountTo, Rate = line.Rate });
            }
            DepositOfferInWork.CalculatingRules.EveryFirstDayOfMonth = DepositOfferDonor.CalculatingRules.EveryFirstDayOfMonth;
            DepositOfferInWork.CalculatingRules.EveryLastDayOfMonth  = DepositOfferDonor.CalculatingRules.EveryLastDayOfMonth ;
            DepositOfferInWork.CalculatingRules.EveryStartDay        = DepositOfferDonor.CalculatingRules.EveryStartDay       ;
            DepositOfferInWork.CalculatingRules.IsCapitalized        = DepositOfferDonor.CalculatingRules.IsCapitalized       ;
            DepositOfferInWork.CalculatingRules.IsFactDays           = DepositOfferDonor.CalculatingRules.IsFactDays          ;
            DepositOfferInWork.CalculatingRules.IsRateFixed          = DepositOfferDonor.CalculatingRules.IsRateFixed         ;
            NotifyOfPropertyChange(() => DepositOfferInWork);

        }

        public void CloseForm()
        {
            TryClose(true);
        }


    }
}