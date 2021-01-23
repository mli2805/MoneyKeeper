using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class RulesAndRatesViewModel : Screen
    {
        public string Title;
        public DepositConditions Conditions { get; set; }
        public ObservableCollection<DepositRateLine> Rows { get; set; }
        public DateTime SelectedDate { get; set; }
        public DateTime NewDate { get; set; } = DateTime.Today;

        public void Initialize(string title, DateTime selectedDate, DepositConditions conditions)
        {
            Title = title;
            SelectedDate = selectedDate;
            Conditions = conditions;
            Rows = new ObservableCollection<DepositRateLine>();
            foreach (var rateLine in conditions.RateLines)
            {
                Rows.Add(rateLine);
            }
            if (Rows.Count == 0)
                Rows.Add(new DepositRateLine(){
                    // DepositOfferId = Conditions.DepositOfferId, 
                    DepositOfferConditionsId = Conditions.Id,
                    DateFrom = DateTime.Today, AmountFrom = 0, AmountTo = 999999999999, Rate = 10});
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Title;
        }

        public void AddLine()
        {
            var lastLine = Rows.Last();
            var newLine = new DepositRateLine()
            {
                // DepositOfferId = lastLine.DepositOfferId,
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
                    // DepositOfferId = line.DepositOfferId,
                    DepositOfferConditionsId = line.DepositOfferConditionsId,
                    DateFrom = NewDate,
                    AmountFrom = line.AmountFrom,
                    AmountTo = line.AmountTo,
                    Rate = line.Rate,
                })
                .ToList();

            foreach (var line in copy)
            {
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
