using System;
using System.Collections.Generic;

namespace Keeper2018
{
    public class DepoPlusCurrencyProvider
    {
        private readonly MonthAnalyser _monthAnalyser;

        public DepoPlusCurrencyProvider(MonthAnalyser monthAnalyser)
        {
            _monthAnalyser = monthAnalyser;
        }

        public void Initialize()
        {
            _monthAnalyser.Initialize();
        }

        public IEnumerable<DepoCurrencyData> Evaluate(int fromYear)
        {
            var start = new DateTime(fromYear, 1, 1);
            while (start < DateTime.Today)
            {
                var mam = _monthAnalyser.Produce(start);
                yield return new DepoCurrencyData()
                {
                    StartDate = start,
                    DepoRevenue = mam.DepoIncome,
                    CurrencyRatesDifferrence = mam.ExchangeDifference,
                };
                start = start.AddMonths(1);
            }
        }
    }
}