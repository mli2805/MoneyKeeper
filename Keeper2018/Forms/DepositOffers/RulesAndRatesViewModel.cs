using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class RulesAndRatesViewModel : Screen
    {
        public string Title;
        public DepositEssential Essential { get; set; }
        public ObservableCollection<DepositRateLine> Rows { get; set; }
        public DateTime SelectedDate { get; set; }
        public DateTime NewDate { get; set; } = DateTime.Today;

        public void Initialize(string title, DateTime selectedDate, DepositEssential essential)
        {
            Title = title;
            SelectedDate = selectedDate;
            Essential = essential;
            Rows = new ObservableCollection<DepositRateLine>();
            foreach (var rateLine in essential.RateLines)
            {
                Rows.Add(rateLine);
            }
            if (Rows.Count == 0)
                Rows.Add(new DepositRateLine(){DateFrom = DateTime.Today, AmountFrom = 0, AmountTo = 999999999999, Rate = 100});
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
                DepositOfferId = lastLine.DepositOfferId,
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
                    DepositOfferId = line.DepositOfferId,
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
            Essential.RateLines = Rows.ToList();
            base.CanClose(callback);
        }

        public void Close()
        {
            TryClose();
        }
    }
}
