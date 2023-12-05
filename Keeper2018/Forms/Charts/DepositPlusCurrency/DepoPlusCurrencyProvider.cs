using System;
using System.Collections.Generic;

namespace Keeper2018
{
    public class DepoPlusCurrencyProvider
    {
        private readonly MonthAnalyzer _monthAnalyser;

        public DepoPlusCurrencyProvider(MonthAnalyzer monthAnalyser)
        {
            _monthAnalyser = monthAnalyser;
        }

        public IEnumerable<DepoCurrencyData> Evaluate(int fromYear)
        {
            var start = new DateTime(fromYear, 1, 1);
            while (start < DateTime.Today)
            {
                var mam = _monthAnalyser.AnalyzeFrom(start);
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