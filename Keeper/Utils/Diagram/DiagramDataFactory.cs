using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Keeper.ByFunctional.AccountEditing;
using Keeper.DomainModel;
using Keeper.Utils.Common;
using Keeper.Utils.DiagramDomainModel;

namespace Keeper.Utils.Diagram
{
    [Export]
    public class DiagramDataFactory
    {
        private readonly KeeperDb _db;
        readonly AccountTreeStraightener mAccountTreeStraightener;
        private DiagramDataExtractorFromDb ExtractorFromDb { get; set; }

        [ImportingConstructor]
        public DiagramDataFactory(KeeperDb db, AccountTreeStraightener accountTreeStraightener)
        {
            _db = db;
            mAccountTreeStraightener = accountTreeStraightener;
            ExtractorFromDb = new DiagramDataExtractorFromDb(db, accountTreeStraightener);
        }

        public DiagramSeries AccountDailyBalancesToSeries(string name, Brush positiveBrush)
        {
            var balancedAccount = (from account in new AccountTreeStraightener().Flatten(_db.Accounts) where account.Name == name select account).FirstOrDefault();
            var balances = ExtractorFromDb.AccountBalancesForPeriodInUsd(balancedAccount, new Period(new DateTime(2001, 12, 31), DateTime.Now), Every.Day);
//            var balances = ExtractorFromDb.AccountInUsdForPeriod(balancedAccount, new Period(new DateTime(2001, 12, 31), DateTime.Now), Every.Day);

            var content = balances.Select(balance => string.Format("{0} {1:F}", balance.Key.Date, balance.Value)).ToList();
            File.WriteAllLines(@"c:\temp\tt.txt", content);

            var data = balances.Select(pair => new DiagramPoint(pair.Key.Date, (double)pair.Value)).ToList();

            return new DiagramSeries
              {
                  Points = data,
                  Index = 0,
                  Name = name,
                  PositiveBrushColor = positiveBrush
              };
        }

        public DiagramDomainModel.DiagramData DailyBalancesCtor()
        {
            var dataForDiagram = new List<DiagramSeries>
                             {
//                               AccountDailyBalancesToSeries("Депозиты", Brushes.LightSkyBlue),
                               AccountDailyBalancesToSeries("Мои", Brushes.Blue)
                             };

            return new DiagramDomainModel.DiagramData
            {
                Caption = "Располагаемые средства",
                Series = dataForDiagram,
                Mode = DiagramMode.Lines,
                TimeInterval = Every.Day
            };
        }

        private Dictionary<DateTime, decimal> ExtractRates(CurrencyCodes currency)
        {
            switch (currency)
            {
                case CurrencyCodes.EUR:
                    //          return _db.CurrencyRates.Where(r => r.Currency == CurrencyCodes.EUR).OrderBy(r => r.BankDay).
                    //                               ToDictionary(currencyRate => currencyRate.BankDay.Date, currencyRate => (decimal)(1 / currencyRate.Rate));


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

        public DiagramDomainModel.DiagramData RatesCtor()
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

            return new DiagramDomainModel.DiagramData
            {
                Caption = "Курсы валют",
                Series = data,
                Mode = DiagramMode.SeparateLines,
                TimeInterval = Every.Day
            };
        }

        public DiagramSeries ArticleMonthlyTrafficToSeries(string name, Brush positiveBrush)
        {
            return new DiagramSeries
                     {
                         Name = name,
                         PositiveBrushColor = positiveBrush,
                         NegativeBrushColor = positiveBrush,
                         Index = 0,
                         Points = (from pair in ExtractorFromDb.MonthlyTraffic(name)
                                 select new DiagramPoint(pair.Key, (double)pair.Value)).ToList()
                     };
        }

        public DiagramDomainModel.DiagramData MonthlyOutcomesDiagramCtor()
        {
            var outcomes = mAccountTreeStraightener.Seek("Все расходы", _db.Accounts);
            var outcomeColors = new List<Brush> {Brushes.LimeGreen, Brushes.DarkGray, Brushes.OrangeRed, Brushes.Magenta, 
                                           Brushes.Yellow, Brushes.Aquamarine, Brushes.DarkOrange, Brushes.DodgerBlue};
            var colorsEnumerator = outcomeColors.GetEnumerator();

            var dataForDiagram = new List<DiagramSeries>();
            foreach (var outcome in outcomes.Children)
            {
                colorsEnumerator.MoveNext();
                dataForDiagram.Add(ArticleMonthlyTrafficToSeries(outcome.Name, colorsEnumerator.Current));
            }

            return new DiagramDomainModel.DiagramData
            {
                Caption = "Ежемесячные расходы в разрезе категорий",
                Series = dataForDiagram,
                Mode = DiagramMode.BarVertical,
                TimeInterval = Every.Month
            };
        }

        public DiagramDomainModel.DiagramData MonthlyIncomesDiagramCtor()
        {
            var dataForDiagram = new List<DiagramSeries>
                             {
                               ArticleMonthlyTrafficToSeries("Зарплата",Brushes.Green),
                               ArticleMonthlyTrafficToSeries("Иррациональные",Brushes.CadetBlue),
                               ArticleMonthlyTrafficToSeries("Рента",Brushes.Blue),
                               ArticleMonthlyTrafficToSeries("Подарки",Brushes.DarkOrange),
                             };

            return new DiagramDomainModel.DiagramData
            {
                Caption = "Ежемесячные доходы (только основные категории)",
                Series = dataForDiagram,
                Mode = DiagramMode.BarVertical,
                TimeInterval = Every.Month
            };
        }

        public DiagramDomainModel.DiagramData MonthlyResultsDiagramCtor()
        {
            var dataForDiagram = new List<DiagramSeries>
                             {
                               new DiagramSeries
                                 {
                                   Name = "Сальдо",
                                   PositiveBrushColor = Brushes.Blue,
                                   NegativeBrushColor = Brushes.Red,
                                   Index = 0,
                                   Points = (from pair in ExtractorFromDb.MonthlyResults("Мои")
                                           select new DiagramPoint(pair.Key, (double) pair.Value)).ToList()
                                 }
                             };

            return new DiagramDomainModel.DiagramData
            {
                Caption = "Сальдо",
                Series = dataForDiagram,
                Mode = DiagramMode.BarVertical,
                TimeInterval = Every.Month
            };
        }

        public DiagramDomainModel.DiagramData AverageSignificancesDiagramCtor()
        {
            var seriesNames = new List<string> { "Все доходы", "Зарплата", "Иррациональные", "Рента", "Все расходы" };
            var seriesColors = new List<Brush> {Brushes.DarkGreen, Brushes.LimeGreen, Brushes.Black, 
                                          Brushes.Blue, Brushes.Red};
            var colorsEnumerator = seriesColors.GetEnumerator();

            var dataForDiagram = new List<DiagramSeries>();
            foreach (var seriesName in seriesNames)
            {
                var originalDictionary = ExtractorFromDb.MonthlyTraffic(seriesName);
                var average12MsDictionary = ExtractorFromDb.Average12MsByDictionary(originalDictionary);

                colorsEnumerator.MoveNext();
                dataForDiagram.Add(new DiagramSeries
                                   {
                                       Name = seriesName,
                                       PositiveBrushColor = colorsEnumerator.Current,
                                       Points = (from pair in average12MsDictionary
                                               select new DiagramPoint(pair.Key.Date, (double)Math.Abs(pair.Value))).ToList()
                                   });
            }

            return new DiagramDomainModel.DiagramData
            {
                Caption = "Средние за 12 месяцев по основным индикативным показателям",
                Series = dataForDiagram,
                Mode = DiagramMode.Lines,
                TimeInterval = Every.Day
            };
        }
    }
}
