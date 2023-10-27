using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Keeper2018
{
    public class SalaryViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;

        private List<SalaryLineModel> _rows = new List<SalaryLineModel>();
        public List<SalaryLineModel> Rows
        {
            get => _rows;
            set
            {
                if (Equals(value, _rows)) return;
                _rows = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isWithIrregulars;
        private bool _isAggregated;

        private List<SalaryLineModel> OnlySalary = new List<SalaryLineModel>();
        private List<SalaryLineModel> SalaryAndIrregulars = new List<SalaryLineModel>();

        private PlotModel _myPlotModel;

        public PlotModel MyPlotModel
        {
            get => _myPlotModel;
            set
            {
                if (Equals(value, _myPlotModel)) return;
                _myPlotModel = value;
                NotifyOfPropertyChange();
            }
        }

        private string _toggleButtonCaption = "Add irregulars";
        public string ToggleButtonCaption
        {
            get => _toggleButtonCaption;
            set
            {
                if (value == _toggleButtonCaption) return;
                _toggleButtonCaption = value;
                NotifyOfPropertyChange();
            }
        } // "Only salary";

        private string _aggregateButtonCaption = "Aggregate";
        public string AggregateButtonCaption
        {
            get => _aggregateButtonCaption;
            set
            {
                if (value == _aggregateButtonCaption) return;
                _aggregateButtonCaption = value;
                NotifyOfPropertyChange();
            }
        } // "In details"

        public SalaryViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Salary";
        }

        public void Initialize()
        {
            var mySalaryFolder = _dataModel.AcMoDict[772];
            BuildFor(mySalaryFolder, OnlySalary);

            var myEmployersFolder = _dataModel.AcMoDict[171];
            BuildFor(myEmployersFolder, SalaryAndIrregulars);

            Rows = SalaryAndIrregulars;
            _isWithIrregulars = true;
            _isAggregated = false;

            MyPlotModel = BuildChart();
        }

        private PlotModel BuildChart()
        {
            var myPlotModel = new PlotModel();
            CreateColumnSeries(myPlotModel);
            myPlotModel.Axes.Add(new CategoryAxis()
            {
                Position = AxisPosition.Bottom,
                MajorStep = 6,
                LabelFormatter = F,
            });
            return myPlotModel;
        }

        private string F(double valueOnAxys)
        {
            var dateTime = new DateTime(2002,1,1).AddMonths((int)valueOnAxys);
            return $"{dateTime:MM/yy}";
        }

        private void CreateColumnSeries(PlotModel myPlotModel)
        {
            var aggr = Aggregate(OnlySalary);
            var aggr2 = Aggregate(SalaryAndIrregulars);

            var salarySeries = new ColumnSeries() { Title = "Salary", FillColor = OxyColors.Blue, IsStacked = true, };
            var irregularSeries = new ColumnSeries() { Title = "Irregular", FillColor = OxyColors.Gray, IsStacked = true, };
            for (int i = 0; i < aggr.Count; i++)
            {
                salarySeries.Items.Add(new ColumnItem((double)aggr[i].AmountInUsd));
                irregularSeries.Items.Add(new ColumnItem((double)(aggr2[i].AmountInUsd - aggr[i].AmountInUsd)));
            }
           
            myPlotModel.Series.Add(salarySeries);
            myPlotModel.Series.Add(irregularSeries);
        }

        private void BuildFor(AccountItemModel accountModelFolder, List<SalaryLineModel> result)
        {
            result.Clear();
            var lines = _dataModel.Transactions
                .Where(t => t.Value.Tags.Select(tt=>tt.Id).ToList()
                    .Intersect(accountModelFolder.Children.Select(c => c.Id)).Any());
            foreach (var keyValuePair in lines)
            {
                result.Add(ToSalaryLine(keyValuePair.Value));
            }
        }

        private List<SalaryLineModel> Aggregate(List<SalaryLineModel> rows)
        {
            var aggr = (from l in rows
                        group l by new { mm = l.Timestamp.Month, yy = l.Timestamp.Year }
                into ag
                        select new SalaryLineModel
                        {
                            IsAggregatedLine = true,
                            Timestamp = new DateTime(ag.Key.yy, ag.Key.mm, 1),
                            AmountInUsd = ag.Sum(l => l.AmountInUsd)
                        }).ToList();

            foreach (var salaryLineModel in rows)
            {
                var line = aggr.First(l =>
                    l.Timestamp.Year == salaryLineModel.Timestamp.Year &&
                    l.Timestamp.Month == salaryLineModel.Timestamp.Month);

                if (line.Employer == null)
                {
                    line.Employer = salaryLineModel.Employer;
                }
                if (line.Amount == null)
                    line.Amount = $"{line.AmountInUsd:0,0} usd";

                if (!string.IsNullOrEmpty(salaryLineModel.Comment))
                {
                    if (!string.IsNullOrEmpty(line.Comment))
                        line.Comment = line.Comment + " ; ";
                    line.Comment = line.Comment + salaryLineModel.Comment;
                }
            }
            return aggr;
        }

        private SalaryLineModel ToSalaryLine(TransactionModel transaction)
        {
            SalaryLineModel result = new SalaryLineModel();
            result.Timestamp = transaction.Timestamp;
            result.Employer = GetEmployer(transaction.Tags);
            result.Amount = _dataModel.AmountInUsdString(transaction.Timestamp, transaction.Currency, transaction.Amount, out decimal amountInUsd);
            result.AmountInUsd = amountInUsd;
            result.Comment = transaction.Comment;
            return result;
        }

        private string GetEmployer(List<AccountItemModel> tags)
        {
            foreach (var tag in tags)
            {
                if (tag.IsTag())
                    continue;
                return tag.Name;
            }
            return "";
        }

        public void ToggleView()
        {
            Rows = _isWithIrregulars ? OnlySalary : SalaryAndIrregulars;
            _isWithIrregulars = !_isWithIrregulars;
            ToggleButtonCaption = _isWithIrregulars ? "Only salary" : "Add irregulars";
        }

        public void AggregateButton()
        {
            Rows = _isAggregated
                ? _isWithIrregulars
                    ? SalaryAndIrregulars
                    : OnlySalary
                : Aggregate(Rows);
            _isAggregated = !_isAggregated;
            AggregateButtonCaption = _isAggregated ? "In details" : "Aggregate";
        }

        public void Close()
        {
            TryClose();
        }
    }
}
