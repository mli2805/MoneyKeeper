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
        public DepositConditions Conditions { get; set; }
        public ObservableCollection<DepositRateLine> Rows { get; set; }
        public DateTime NewDate { get; set; } = DateTime.Today;

        public List<DateTime> _conditionDateTimes;

        public void Initialize(string title, DepositConditions conditions, KeeperDataModel keeperDataModel, List<DateTime> conditionDateTimes)
        {
            Title = title;
            Conditions = conditions;
            _conditionDateTimes = conditionDateTimes;
            Rows = new ObservableCollection<DepositRateLine>();
            foreach (var rateLine in conditions.RateLines)
            {
                Rows.Add(rateLine);
            }
            if (Rows.Count == 0)
                Rows.Add(new DepositRateLine(){
                    Id = keeperDataModel.GetDepoRateLinesMaxId() + 1,
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
            var newLine = new DepositRateLine()
            {
                Id = lastLine.Id + 1,
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

            foreach (var line in copy)
            {
                Rows.Add(line);
            }
        }

        public override void CanClose(Action<bool> callback)
        {
            if (_conditionDateTimes.Contains(Conditions.DateFrom))
            {
                MessageBox.Show("Уже есть условия действующие с этой даты.");
                return;
            }
            Conditions.RateLines = Rows.ToList();
            base.CanClose(callback);
        }

        public void Close()
        {
            TryClose();
        }
    }
}
