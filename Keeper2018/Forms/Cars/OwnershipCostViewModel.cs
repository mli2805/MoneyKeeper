using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Keeper2018
{
    public class OwnershipCostViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private CarModel _carModel;

        public PlotModel DailyOwnershipCostPlotModel { get; set; }
        public PlotModel MonthlyOwnershipCostPlotModel { get; set; }

        private Visibility _dailyVisibility = Visibility.Visible;
        public Visibility DailyVisibility
        {
            get => _dailyVisibility;
            set
            {
                if (value == _dailyVisibility) return;
                _dailyVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private Visibility _monthlyVisibility = Visibility.Collapsed;
        public Visibility MonthlyVisibility
        {
            get => _monthlyVisibility;
            set
            {
                if (value == _monthlyVisibility) return;
                _monthlyVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public OwnershipCostViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Initialize(CarModel carModel)
        {
            _carModel = carModel;

            var carAccountModel = _dataModel.AcMoDict[_carModel.CarAccountId];
            var trans = _dataModel.Transactions.Values.OrderBy(t => t.Timestamp)
                .Where(m => m.Tags.Any(tag => tag.Is(carAccountModel))).ToList();

            DailyOwnershipCostPlotModel = new PlotModel();
            SetAxis(DailyOwnershipCostPlotModel);
            DailyOwnershipCostPlotModel.Series.Add(OwnershipCostForDay(trans));

            MonthlyOwnershipCostPlotModel = new PlotModel();
            SetAxis(MonthlyOwnershipCostPlotModel);
            MonthlyOwnershipCostPlotModel.Series.Add(OwnershipCostForMonth(trans));
        }

        private void SetAxis(PlotModel plotModel)
        {
            plotModel.Axes.Add(new DateTimeAxis()
            {
                Position = AxisPosition.Bottom,
                IntervalLength = 75,
                MinorIntervalType = DateTimeIntervalType.Days,
                IntervalType = DateTimeIntervalType.Days,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dash,
            });
            plotModel.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Automatic,
                MinorGridlineStyle = LineStyle.Automatic,
            });
        }

        private LineSeries OwnershipCostForMonth(List<TransactionModel> trans)
        {
            var mainSeries = new LineSeries()
            {
                Title = "Стоимость владения в месяц",
                Color = OxyColors.Blue,
            };

            DateTime firstDate = trans.First().Timestamp.Date;
            DateTime currentDate = firstDate;
            decimal sumInUsd = 0;
            foreach (var transactionModel in trans)
            {
                while (Months(firstDate, transactionModel.Timestamp.Date) > Months(firstDate, currentDate.Date))
                {
                    var months = Months(firstDate.Date, currentDate.Date) + 1;
                    var point = new DataPoint(DateTimeAxis.ToDouble(currentDate), (double)sumInUsd / months);
                    mainSeries.Points.Add(point);
                    currentDate = currentDate.AddMonths(1);
                }

                if (transactionModel.Timestamp.Date == currentDate)
                {
                    if (transactionModel.Tags.Any(tag => tag.Id == 707 || tag.Id == 709 || tag.Id == 713 || tag.Id == 717))
                        continue;

                    sumInUsd += transactionModel.GetAmountInUsd(_dataModel);
                }
            }

            var monthsL = Months(firstDate.Date, currentDate.Date) + 1;
            var pointL = new DataPoint(DateTimeAxis.ToDouble(currentDate), (double)sumInUsd / monthsL);
            mainSeries.Points.Add(pointL);
            return mainSeries;
        }

        private LineSeries OwnershipCostForDay(List<TransactionModel> trans)
        {
            var mainSeries = new LineSeries()
            {
                Title = "Стоимость владения в день",
                Color = OxyColors.BlueViolet,
            };

            DateTime firstDate = trans.First().Timestamp.Date;
            DateTime currentDate = firstDate;
            decimal sumInUsd = 0;
            foreach (var transactionModel in trans)
            {
                while (transactionModel.Timestamp.Date > currentDate.Date)
                {
                    var days = (currentDate.Date - firstDate.Date).Days + 1;
                    var point = new DataPoint(DateTimeAxis.ToDouble(currentDate), (double)sumInUsd / days);
                    mainSeries.Points.Add(point);
                    currentDate = currentDate.AddDays(1);
                }

                if (transactionModel.Timestamp.Date == currentDate)
                {
                    if (transactionModel.Tags.Any(tag => tag.Id == 707 || tag.Id == 709 || tag.Id == 713 || tag.Id == 717))
                        continue;

                    sumInUsd += transactionModel.GetAmountInUsd(_dataModel);
                }

            }

            var daysL = (currentDate.Date - firstDate.Date).Days + 1;
            var pointL = new DataPoint(DateTimeAxis.ToDouble(currentDate), (double)sumInUsd / daysL);
            mainSeries.Points.Add(pointL);
            return mainSeries;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = $"Стоимость владения {_carModel.Title}  (T - переключить день/месяц)";
        }

        private int _model = 1;
        public void ToggleModel()
        {
            if (_model == 1)
            {
                _model = 2;
                DailyVisibility = Visibility.Collapsed;
                MonthlyVisibility = Visibility.Visible;
            }
            else
            {
                _model = 1;
                DailyVisibility = Visibility.Visible;
                MonthlyVisibility = Visibility.Collapsed;
            }
        }

        private static int Months(DateTime firstDate, DateTime secondDate)
        {
            var count = 0;
            var date = firstDate;
            while (date < secondDate)
            {
                date = date.AddMonths(1);
                count++;
            }
            return count;
        }
    }
}
