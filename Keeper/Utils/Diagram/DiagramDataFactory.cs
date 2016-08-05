using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.Utils.AccountEditing;
using Keeper.Utils.DiagramDomainModel;
using Keeper.Utils.OxyPlots;

namespace Keeper.Utils.Diagram
{
    [Export]
    public class DiagramDataFactory
    {
        private readonly KeeperDb _db;
        readonly AccountTreeStraightener mAccountTreeStraightener;
        readonly DataClassifiedByCategoriesProvider _dataClassifiedByCategoriesProvider;
        private readonly DiagramDataExtractorFromDb _diagramDataExtractorFromDb;

        [ImportingConstructor]
        public DiagramDataFactory(KeeperDb db, AccountTreeStraightener accountTreeStraightener,
            DataClassifiedByCategoriesProvider dataClassifiedByCategoriesProvider, DiagramDataExtractorFromDb diagramDataExtractorFromDb)
        {
            _db = db;
            _dataClassifiedByCategoriesProvider = dataClassifiedByCategoriesProvider;
            _diagramDataExtractorFromDb = diagramDataExtractorFromDb;
            mAccountTreeStraightener = accountTreeStraightener;
        }

        public DiagramSeries AccountDailyBalancesToSeries(string name, Brush positiveBrush)
        {
            return new DiagramSeries
            {
                Points = _diagramDataExtractorFromDb.GetBalances(Every.Day),
                Index = 0,
                Name = name,
                PositiveBrushColor = positiveBrush
            };
        }

        public DiagramData DailyBalancesCtor()
        {
            return new DiagramData
            {
                Caption = "Располагаемые средства",
                Series = new List<DiagramSeries>
                {
                    new DiagramSeries
                    {
                        Points = _diagramDataExtractorFromDb.GetBalances(Every.Day),
                        Index = 0,
                        Name = "Мои",
                        PositiveBrushColor = Brushes.Blue
                    },
                },
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

        public DiagramData MonthlyIncomesDiagramCtor()
        {
            var categoriesList = new List<Account>
            {
                mAccountTreeStraightener.Seek("Зарплата", _db.Accounts),
                mAccountTreeStraightener.Seek("Иррациональные", _db.Accounts),
                mAccountTreeStraightener.Seek("Рента", _db.Accounts),
                mAccountTreeStraightener.Seek("Подарки", _db.Accounts),
            };

            var colors = new List<Brush> { Brushes.Green, Brushes.DarkGray, Brushes.Blue, Brushes.DarkOrange };
            var colorsEnumerator = colors.GetEnumerator();

            var data = _dataClassifiedByCategoriesProvider.GetGrouppedByMonth(true);

            var dataForDiagram = CategoryDiagramCtor(categoriesList, colorsEnumerator, data, 1);

            return new DiagramData
            {
                Caption = "Ежемесячные доходы (только основные категории)",
                Series = dataForDiagram,
                Mode = DiagramMode.BarVertical,
                TimeInterval = Every.Month
            };
        }

        public DiagramData AverageOfMainCategoriesDiagramCtor()
        {
            var categoriesList = new List<Account>
            {
                mAccountTreeStraightener.Seek("Зарплата", _db.Accounts),
                mAccountTreeStraightener.Seek("Иррациональные", _db.Accounts),
                mAccountTreeStraightener.Seek("Рента", _db.Accounts),
            };
            var colors = new List<Brush> { Brushes.LimeGreen, Brushes.Black, Brushes.Blue };
            var colorsEnumerator = colors.GetEnumerator();

            var expenseData = _dataClassifiedByCategoriesProvider.GetGrouppedByMonth(false);
            var incomeData = _dataClassifiedByCategoriesProvider.GetGrouppedByMonth(true);

            var dataForDiagram = CategoryDiagramCtor(categoriesList, colorsEnumerator, incomeData, 1);
            dataForDiagram.Add(SumAllIncomes(incomeData));
            dataForDiagram.Add(SumAllExpense(expenseData));

            Evaluate12MonthsAverageInsteadOfExistingData(dataForDiagram);

            return new DiagramData
            {
                Caption = "Средние за 12 месяцев по основным индикативным показателям",
                Series = dataForDiagram,
                Mode = DiagramMode.Lines,
                TimeInterval = Every.Day
            };

        }

        private void Evaluate12MonthsAverageInsteadOfExistingData(List<DiagramSeries> data)
        {
            foreach (var series in data)
            {
                var temp = Evaluate12MonthsAverage(series.Points);
                series.Points = temp;
            }
        }

        private List<DiagramPoint> SumCategories(List<DataClassifiedByCategoriesElement> dataByCategories)
        {
            return
                (from element in dataByCategories
                 group element by element.YearMonth into g
                 select new DiagramPoint() { CoorXdate = g.Key.LastDay(), CoorYdouble = (double)g.Sum(e => e.Amount) }).ToList();
        }

        private DiagramSeries SumAllIncomes(List<DataClassifiedByCategoriesElement> dataByCategories)
        {
            return new DiagramSeries
            {
                Name = "Все доходы",
                PositiveBrushColor = Brushes.DarkGreen,
                NegativeBrushColor = Brushes.DarkGreen,
                Index = 0,
                Points = SumCategories(dataByCategories),
            };
        }

        private DiagramSeries SumAllExpense(List<DataClassifiedByCategoriesElement> dataByCategories)
        {
            return new DiagramSeries
            {
                Name = "Все расходы",
                PositiveBrushColor = Brushes.Red,
                NegativeBrushColor = Brushes.Red,
                Index = 0,
                Points = SumCategories(dataByCategories),
            };
        }

        private List<DiagramPoint> Evaluate12MonthsAverage(List<DiagramPoint> inputData)
        {
            var result = new List<DiagramPoint>();
            var last12MonthsValues = new List<DiagramPoint>();
            foreach (var point in inputData.OrderBy(p => p.CoorXdate))
            {
                if (last12MonthsValues.Count == 12)
                    last12MonthsValues.Remove(last12MonthsValues.OrderBy(p => p.CoorXdate).FirstOrDefault());

                last12MonthsValues.Add(point);
                var averageForLast12Months = last12MonthsValues.Sum(p => p.CoorYdouble) / last12MonthsValues.Count;

                result.Add(new DiagramPoint() { CoorXdate = point.CoorXdate, CoorYdouble = averageForLast12Months });
            }
            return result;

        }

        public DiagramData MonthlyExpenseDiagramCtor()
        {
            var root = mAccountTreeStraightener.Seek("Все расходы", _db.Accounts);

            var outcomeColors = new List<Brush> {Brushes.LimeGreen, Brushes.DarkGray, Brushes.OrangeRed, Brushes.Magenta,
                                           Brushes.Yellow, Brushes.Aquamarine, Brushes.DarkOrange, Brushes.DodgerBlue};
            var colorsEnumerator = outcomeColors.GetEnumerator();

            var data = _dataClassifiedByCategoriesProvider.GetGrouppedByMonth(false);

            var dataForDiagram = CategoryDiagramCtor(root.Children, colorsEnumerator, data, -1);

            return new DiagramData
            {
                Caption = "Ежемесячные расходы в разрезе категорий",
                Series = dataForDiagram,
                Mode = DiagramMode.BarVertical,
                TimeInterval = Every.Month
            };
        }

        private List<DiagramSeries> CategoryDiagramCtor(IEnumerable<Account> categoriesList, List<Brush>.Enumerator colorsEnumerator, List<DataClassifiedByCategoriesElement> data, double sign)
        {
            var result = new List<DiagramSeries>();
            foreach (var category in categoriesList)
            {
                colorsEnumerator.MoveNext();
                var categoryDate = data.Where(t => t.Category == category);

                result.Add(new DiagramSeries
                {
                    Name = category.Name,
                    PositiveBrushColor = colorsEnumerator.Current,
                    NegativeBrushColor = colorsEnumerator.Current,
                    Index = 0,
                    Points =
                        (from line in categoryDate
                         select new DiagramPoint(line.YearMonth.LastDay(), (double)line.Amount * sign)).ToList(),
                });
            }
            return result;
        }

        public DiagramData MonthlyResultsDiagramCtor()
        {
            var dataForDiagram = new List<DiagramSeries>
                             {
                               new DiagramSeries
                                 {
                                   Name = "Сальдо",
                                   PositiveBrushColor = Brushes.Blue,
                                   NegativeBrushColor = Brushes.Red,
                                   Index = 0,
                                   Points = _diagramDataExtractorFromDb.MonthlyResults(),
                                 }
                             };

            return new DiagramData
            {
                Caption = "Сальдо",
                Series = dataForDiagram,
                Mode = DiagramMode.BarVertical,
                TimeInterval = Every.Month
            };
        }
    }
}
