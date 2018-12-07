using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class RulesAndRatesViewModel : Screen
    {
        public DepositEssential Essential { get; set; }
        public ObservableCollection<DepositRateLine> Rows { get; set; }

        public void Initialize(DepositEssential essential)
        {
            Rows = new ObservableCollection<DepositRateLine>();
            foreach (var rateLine in essential.RateLines)
            {
                Rows.Add(rateLine);
            }
            if (Rows.Count == 0)
                Rows.Add(new DepositRateLine(){DateFrom = DateTime.Today, AmountFrom = 0, AmountTo = 999999999999, Rate = 100});
        }

        public void Close()
        {
            Essential.RateLines = Rows.ToList();
        }
    }
}
