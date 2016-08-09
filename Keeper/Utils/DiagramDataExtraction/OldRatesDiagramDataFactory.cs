using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.Utils.DiagramDomainModel;

namespace Keeper.Utils.DiagramDataExtraction
{
    [Export]
    public class OldRatesDiagramDataFactory
    {
        private readonly KeeperDb _db;

        [ImportingConstructor]
        public OldRatesDiagramDataFactory(KeeperDb db)
        {
            _db = db;
        }

        private Dictionary<DateTime, decimal> ExtractRates(CurrencyCodes currency)
        {
            switch (currency)
            {
                case CurrencyCodes.EUR:
                    var ddd = _db.CurrencyRates.Where(r => r.Currency == CurrencyCodes.EUR).OrderBy(r => r.BankDay);
                    var di = new Dictionary<DateTime, decimal>();
                    foreach (var currencyRate in ddd)
                    {
                        if (di.ContainsKey(currencyRate.BankDay.Date)) MessageBox.Show(string.Format("{0}", currencyRate.BankDay.Date));
                        else di.Add(currencyRate.BankDay.Date, (decimal)(1 / currencyRate.Rate));

                    }
                    return di;
                case CurrencyCodes.BYR:
                    return _db.CurrencyRates.Where(r => r.Currency == CurrencyCodes.BYR).OrderBy(r => r.BankDay).
                                         ToDictionary(currencyRate => currencyRate.BankDay.Date, currencyRate => (decimal)currencyRate.Rate);
                default:
                    return null;
            }
        }

        private Dictionary<DateTime, decimal> FillinGapsInRates(Dictionary<DateTime, decimal> ratesFromDb)
        {
            var result = new Dictionary<DateTime, decimal>();
            var previousPair = new KeyValuePair<DateTime, decimal>(new DateTime(0), 0);
            foreach (var pair in ratesFromDb)
            {
                if (!previousPair.Key.Equals(new DateTime(0)))
                {

                    var interval = (pair.Key - previousPair.Key).Days;
                    if (interval > 1)
                    {
                        var delta = (pair.Value - previousPair.Value) / interval;
                        for (int i = 1; i < interval; i++)
                        {
                            result.Add(previousPair.Key.AddDays(i), previousPair.Value + delta * i);
                        }
                    }
                }
                result.Add(pair.Key, pair.Value);
                previousPair = pair;
            }
            return result;
        }

        private Dictionary<DateTime, decimal> ByrRateLogarithm(Dictionary<DateTime, decimal> byrRates)
        {
            return byrRates.ToDictionary(byrRate => byrRate.Key, byrRate => (decimal)Math.Log10((double)byrRate.Value));
        }

        public DiagramData RatesCtor()
        {
            var data = new List<DiagramSeries>();

            var byrRates = FillinGapsInRates(ExtractRates(CurrencyCodes.BYR));
            data.Add(new DiagramSeries
            {
                Name = "BYR",
                PositiveBrushColor = Brushes.Brown,
                Points = (from pair in byrRates select new DiagramPoint(pair.Key.Date, (double)pair.Value)).ToList()
            });

            var byrRatesLog = ByrRateLogarithm(byrRates);
            data.Add(new DiagramSeries
            {
                Name = "LogBYR",
                PositiveBrushColor = Brushes.Chocolate,
                Points = (from pair in byrRatesLog select new DiagramPoint(pair.Key.Date, (double)pair.Value)).ToList()
            });

            var euroRates = FillinGapsInRates(ExtractRates(CurrencyCodes.EUR));
            data.Add(new DiagramSeries
            {
                Name = "EURO",
                PositiveBrushColor = Brushes.Blue,
                Points = (from pair in euroRates select new DiagramPoint(pair.Key.Date, (double)pair.Value)).ToList()
            });

            return new DiagramData
            {
                Caption = "Курсы валют",
                Series = data,
                Mode = DiagramMode.SeparateLines,
                TimeInterval = Every.Day
            };
        }

    }
}
