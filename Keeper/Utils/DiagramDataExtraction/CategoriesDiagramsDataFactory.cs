using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Windows.Media;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.Utils.AccountEditing;
using Keeper.Utils.DiagramDomainModel;

namespace Keeper.Utils.DiagramDataExtraction
{
    [Export]
    public class CategoriesDiagramsDataFactory
    {
        private readonly KeeperDb _db;
        private readonly AccountTreeStraightener _accountTreeStraightener;
        private readonly CategoriesDataExtractor _categoriesDataExtractor;

        [ImportingConstructor]
        public CategoriesDiagramsDataFactory(KeeperDb db, AccountTreeStraightener accountTreeStraightener, CategoriesDataExtractor categoriesDataExtractor)
        {
            _db = db;
            _accountTreeStraightener = accountTreeStraightener;
            _categoriesDataExtractor = categoriesDataExtractor;
        }


        public DiagramData MonthlyIncomesDiagramCtor()
        {
            var categoriesList = new List<Account>
            {
                _accountTreeStraightener.Seek("Зарплата", _db.Accounts),
                _accountTreeStraightener.Seek("Иррациональные", _db.Accounts),
                _accountTreeStraightener.Seek("Рента", _db.Accounts),
                _accountTreeStraightener.Seek("Подарки", _db.Accounts),
            };

            var colors = new List<Brush> { Brushes.Green, Brushes.DarkGray, Brushes.Blue, Brushes.DarkOrange };
            var colorsEnumerator = colors.GetEnumerator();

            var data = _categoriesDataExtractor.GetGrouppedByMonth(true);

            var dataForDiagram = GroupByCategories(categoriesList, colorsEnumerator, data, 1);

            return new DiagramData
            {
                Caption = "Ежемесячные доходы (только основные категории)",
                Series = dataForDiagram,
                Mode = DiagramMode.BarVertical,
                TimeInterval = Every.Month
            };
        }

        public DiagramData MonthlyExpenseDiagramCtor()
        {
            var root = _accountTreeStraightener.Seek("Все расходы", _db.Accounts);

            var outcomeColors = new List<Brush> {Brushes.LimeGreen, Brushes.DarkGray, Brushes.OrangeRed, Brushes.Magenta,
                Brushes.Yellow, Brushes.Aquamarine, Brushes.DarkOrange, Brushes.DodgerBlue};
            var colorsEnumerator = outcomeColors.GetEnumerator();

            var data = _categoriesDataExtractor.GetGrouppedByMonth(false);

            var dataForDiagram = GroupByCategories(root.Children, colorsEnumerator, data, -1);

            return new DiagramData
            {
                Caption = "Ежемесячные расходы в разрезе категорий",
                Series = dataForDiagram,
                Mode = DiagramMode.BarVertical,
                TimeInterval = Every.Month
            };
        }

        private List<DiagramSeries> GroupByCategories(IEnumerable<Account> categoriesList, List<Brush>.Enumerator colorsEnumerator, List<CategoriesDataElement> data, double sign)
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

        public DiagramData AverageOfMainCategoriesDiagramCtor()
        {
            var categoriesList = new List<Account>
            {
                _accountTreeStraightener.Seek("Зарплата", _db.Accounts),
                _accountTreeStraightener.Seek("Иррациональные", _db.Accounts),
                _accountTreeStraightener.Seek("Рента", _db.Accounts),
            };
            var colors = new List<Brush> { Brushes.LimeGreen, Brushes.Black, Brushes.Blue };
            var colorsEnumerator = colors.GetEnumerator();

            var expenseData = _categoriesDataExtractor.GetGrouppedByMonth(false);
            var incomeData = _categoriesDataExtractor.GetGrouppedByMonth(true);

            var dataForDiagram = GroupByCategories(categoriesList, colorsEnumerator, incomeData, 1);
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

        private DiagramSeries SumAllIncomes(List<CategoriesDataElement> dataByCategories)
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

        private DiagramSeries SumAllExpense(List<CategoriesDataElement> dataByCategories)
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
        private List<DiagramPoint> SumCategories(List<CategoriesDataElement> dataByCategories)
        {
            return
                (from element in dataByCategories
                    group element by element.YearMonth into g
                    select new DiagramPoint() { CoorXdate = g.Key.LastDay(), CoorYdouble = (double)g.Sum(e => e.Amount) }).ToList();
        }

    }
}
