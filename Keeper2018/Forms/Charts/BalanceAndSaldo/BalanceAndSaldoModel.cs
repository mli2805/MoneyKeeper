using System;
using System.Collections.Generic;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Keeper2018
{
    public class BalanceAndSaldoModel
    {
        private KeeperDb _db;
        public LineSeries DailyBalancesSeries { get; set; }
        public ColumnSeries MonthlySaldoSeries { get; set; }
        public ColumnSeries AnnualSaldoSeries { get; set; }

        public List<string> MonthLabels = new List<string>();
        public List<string> YearLabels = new List<string>();

        public void Initialize(KeeperDb db)
        {
            _db = db;
            DailyBalancesSeries = new LineSeries()
            { Title = "Дневные остатки", Color = OxyColors.BlueViolet };
            MonthlySaldoSeries = new ColumnSeries()
            { Title = "Сальдо по месяцам", FillColor = OxyColors.Blue, NegativeFillColor = OxyColors.Red };
            AnnualSaldoSeries = new ColumnSeries()
            { Title = "Сальдо по годам", FillColor = OxyColors.Blue, NegativeFillColor = OxyColors.Red };
        }

        private Balance _balance = new Balance();
        private int _monthIndex;
        private int _yearIndex;
        private double _previousMonth;
        private double _previousYear;
        private double _balanceInUsd;
        private DateTime _currentDate = new DateTime(2001, 12, 31);
        private DateTime _previousDate;
        public void PrepareSeries()
        {
            foreach (var tran in _db.Bin.Transactions.Values)
            {
                if (!tran.Timestamp.Date.Equals(_currentDate))
                {
                    RegisterDay(tran);
                }
                RegisterTran(tran);
            }
            RegisterDay(null);
        }

        private void RegisterDay(Transaction tran)
        {
            _balanceInUsd = (double) _db.BalanceInUsd(_currentDate, _balance);
            DailyBalancesSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(_currentDate), _balanceInUsd));

            _previousDate = _currentDate;
            _currentDate = tran?.Timestamp.Date ?? _currentDate.AddMonths(13);

            if (_currentDate.Month != _previousDate.Month)
            {
                if (_previousDate.Year != 2001)
                {
                    MonthLabels.Add(_previousDate.ToString("MM/yyyy"));
                    MonthlySaldoSeries.Items.Add(new ColumnItem(_balanceInUsd - _previousMonth, _monthIndex));
                    _monthIndex++;
                }

                _previousMonth = _balanceInUsd;
            }

            if (_currentDate.Year != _previousDate.Year)
            {
                if (_previousDate.Year != 2001)
                {
                    YearLabels.Add(_previousDate.ToString("yyyy"));
                    AnnualSaldoSeries.Items.Add(new ColumnItem(_balanceInUsd - _previousYear, _yearIndex));
                    _yearIndex++;
                }

                _previousYear = _balanceInUsd;
            }
        }

        private void RegisterTran(Transaction tran)
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
    }
}