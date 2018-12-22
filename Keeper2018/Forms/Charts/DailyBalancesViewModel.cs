using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
#pragma warning disable 612

namespace Keeper2018
{
    public class DailyBalancesViewModel : Screen
    {
        private KeeperDb _db;
        public PlotModel DailyBalancesModel { get; set; }
        public LineSeries DailyBalancesSeries { get; set; }

        public Visibility DailyBalancesModelVisibility
        {
            get { return _dailyBalancesModelVisibility; }
            set
            {
                if (value == _dailyBalancesModelVisibility) return;
                _dailyBalancesModelVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public PlotModel MonthlySaldoModel { get; set; }
        public ColumnSeries MonthlySaldoSeries { get; set; }

        public Visibility MonthlySaldoModelVisibility
        {
            get { return _monthlySaldoModelVisibility; }
            set
            {
                if (value == _monthlySaldoModelVisibility) return;
                _monthlySaldoModelVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public PlotModel AnnualSaldoModel { get; set; }
        public ColumnSeries AnnualSaldoSeries { get; set; }

        public Visibility AnnualSaldoModelVisibility
        {
            get { return _annualSaldoModelVisibility; }
            set
            {
                if (value == _annualSaldoModelVisibility) return;
                _annualSaldoModelVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public void Initialize(KeeperDb db)
        {
            _db = db;

            InitializeDailyBalances();
            InitializeMonthlySaldo();

            PrepareSeries();

            DailyBalancesModel.Series.Add(DailyBalancesSeries);
            MonthlySaldoModel.Series.Add(MonthlySaldoSeries);
            AnnualSaldoModel.Series.Add(AnnualSaldoSeries);
        }

        private void InitializeDailyBalances()
        {
            DailyBalancesModel = new PlotModel();
            DailyBalancesSeries = new LineSeries() { Title = "Дневные остатки", Color = OxyColors.BlueViolet };

            DailyBalancesModel.Axes.Add(new DateTimeAxis()
            {
                Position = AxisPosition.Bottom,
                IntervalLength = 75,
                MinorIntervalType = DateTimeIntervalType.Days,
                IntervalType = DateTimeIntervalType.Days,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dash,
            });
            DailyBalancesModel.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Left, 
                MajorGridlineStyle = LineStyle.Automatic,
                MinorGridlineStyle = LineStyle.Automatic,
            });
        }

        private void InitializeMonthlySaldo()
        {
            MonthlySaldoModel = new PlotModel();
            MonthlySaldoSeries = new ColumnSeries() { Title = "Сальдо по месяцам", FillColor = OxyColors.Blue, NegativeFillColor = OxyColors.Red };
            AnnualSaldoModel = new PlotModel();
            AnnualSaldoSeries = new ColumnSeries()
            { Title = "Сальдо по годам", FillColor = OxyColors.Blue, NegativeFillColor = OxyColors.Red };
            MonthlySaldoModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Dash });
            AnnualSaldoModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Dash });
        }

        private Balance _balance = new Balance();
        private Visibility _dailyBalancesModelVisibility = Visibility.Visible;
        private Visibility _monthlySaldoModelVisibility = Visibility.Collapsed;
        private Visibility _annualSaldoModelVisibility = Visibility.Collapsed;

        private void PrepareSeries()
        {
            var monthLabels = new List<string>();
            var yearLabels = new List<string>();
            int monthIndex = 0;
            int yearIndex = 0; 
            double previousMonth = 0.0;
            double previousYear = 0.0;
            double balanceInUsd = 0.0, value;
            var currentDate = new DateTime(2001, 12, 31);
            foreach (var tran in _db.TransactionModels)
            {
                if (!tran.Timestamp.Date.Equals(currentDate))
                {
                    var day = DateTimeAxis.ToDouble(currentDate);
                    balanceInUsd = (double)_db.BalanceInUsd(currentDate, _balance);
                    DailyBalancesSeries.Points.Add(new DataPoint(day, balanceInUsd));

                    var previousDate = currentDate;
                    currentDate = tran.Timestamp.Date;

                    if (currentDate.Month != previousDate.Month)
                    {
                        if (previousDate.Year != 2001)
                        {
                            monthLabels.Add(previousDate.ToString("MM/yyyy"));
                            value = balanceInUsd - previousMonth;
                            MonthlySaldoSeries.Items.Add(new ColumnItem(value, monthIndex));
                            monthIndex++;
                        }
                        previousMonth = balanceInUsd;
                    }
                    if (currentDate.Year != previousDate.Year)
                    {
                        if (previousDate.Year != 2001)
                        {
                            yearLabels.Add(previousDate.ToString("yyyy"));
                            value = balanceInUsd - previousYear;
                            AnnualSaldoSeries.Items.Add(new ColumnItem(value, yearIndex));
                            yearIndex++;
                        }
                        previousYear = balanceInUsd;
                    }
                }
                RegisterTran(tran);
            }
            monthLabels.Add(currentDate.ToString("MM/yyyy"));
            MonthlySaldoSeries.Items.Add(new ColumnItem(balanceInUsd - previousMonth, monthIndex));
            yearLabels.Add(currentDate.ToString("yyyy"));
            AnnualSaldoSeries.Items.Add(new ColumnItem(balanceInUsd - previousYear, yearIndex));
                               
            MonthlySaldoModel.Axes.Add(new CategoryAxis(null, monthLabels.ToArray()));
            AnnualSaldoModel.Axes.Add(new CategoryAxis(null, yearLabels.ToArray()));
        }

        private void RegisterTran(TransactionModel tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход:
                    _balance.Add(tran.Currency, tran.Amount);
                    break;
                case OperationType.Расход:
                    _balance.Sub(tran.Currency, tran.Amount);
                    break;
                case OperationType.Перенос:
                    if (tran.Timestamp.Date.Year == 2001)
                        _balance.Add(tran.Currency, tran.Amount);
                    break;
                case OperationType.Обмен:
                    _balance.Sub(tran.Currency, tran.Amount);
                    // ReSharper disable once PossibleInvalidOperationException
                    _balance.Add((CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                    break;
            }
        }

        private int _model = 1;
        public void ToggleModel()
        {
            if (_model == 1)
            {
                _model = 2;
                DailyBalancesModelVisibility = Visibility.Collapsed;
                MonthlySaldoModelVisibility = Visibility.Visible;
                AnnualSaldoModelVisibility = Visibility.Collapsed;
            }
            else if (_model == 2)
            {
                _model = 3;
                DailyBalancesModelVisibility = Visibility.Collapsed;
                MonthlySaldoModelVisibility = Visibility.Collapsed;
                AnnualSaldoModelVisibility = Visibility.Visible;
            }
            else
            {
                _model = 1;
                DailyBalancesModelVisibility = Visibility.Visible;
                MonthlySaldoModelVisibility = Visibility.Collapsed;
                AnnualSaldoModelVisibility = Visibility.Collapsed;
            }
        }
    }
}
