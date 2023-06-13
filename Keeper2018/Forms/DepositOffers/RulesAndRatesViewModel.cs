using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class RulesAndRatesViewModel : Screen
    {
        public string Title;
        public DepoCondsModel Conditions { get; set; }
        public ObservableCollection<DepositRateLine> Rows { get; set; }
        public DateTime NewDate { get; set; } = DateTime.Today;
        private int _maxDepoRateLineId;

        public Visibility FormulaVisibility { get; set; }

        public List<string> Operations { get; set; } = new List<string>() { "*", "+", "/", "-" };
        public string SelectedOperation { get; set; }
        public double FormulaK { get; set; }

        public void Initialize(string title, DepoCondsModel conditions, RateType rateType, int maxDepoRateLineId)
        {
            Title = title;
            Conditions = conditions;
            _maxDepoRateLineId = maxDepoRateLineId;

            FormulaVisibility = rateType == RateType.Linked ? Visibility.Visible : Visibility.Collapsed;
            if (rateType == RateType.Linked)
            {
                RateFormula.TryParse(conditions.RateFormula, out string op, out double k);
                SelectedOperation = Operations.First(o => o == op);
                FormulaK = k;
            }
           

            Rows = new ObservableCollection<DepositRateLine>();
            foreach (var rateLine in conditions.RateLines)
            {
                Rows.Add(rateLine);
            }
            if (Rows.Count == 0)
                Rows.Add(new DepositRateLine()
                {
                    Id = maxDepoRateLineId + 1,
                    DepositOfferConditionsId = Conditions.Id,
                    DateFrom = DateTime.Today,
                    AmountFrom = 0,
                    AmountTo = 999999999999,
                    Rate = 0
                });

        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Title;
        }

        public void AddLine()
        {
            var lastLine = Rows.Last();
            var id = Math.Max(_maxDepoRateLineId, Rows.Max(r => r.Id));
            var newLine = new DepositRateLine()
            {
                Id = id + 1,
                DepositOfferConditionsId = lastLine.DepositOfferConditionsId,
                DateFrom = lastLine.DateFrom,
                AmountFrom = lastLine.AmountTo + (decimal)0.01,
                AmountTo = lastLine.AmountTo * 100 - (decimal)0.01,
                Rate = lastLine.Rate,
            };
            Rows.Add(newLine);
        }

        public void RepeatDay()
        {
            var lastLine = Rows.Last();
            var copy = Rows.Where(r => r.DateFrom == lastLine.DateFrom)
                .Select(line => new DepositRateLine()
                {
                    DepositOfferConditionsId = line.DepositOfferConditionsId,
                    DateFrom = NewDate,
                    AmountFrom = line.AmountFrom,
                    AmountTo = line.AmountTo,
                    Rate = line.Rate,
                })
                .ToList();

            var id = Math.Max(_maxDepoRateLineId, Rows.Max(r => r.Id));
            foreach (var line in copy)
            {
                line.Id = ++id;
                Rows.Add(line);
            }
        }

        public override void CanClose(Action<bool> callback)
        {
            Conditions.RateLines = Rows.ToList();
            base.CanClose(callback);
        }

        public void Close()
        {
            TryClose();
        }
    }
}
