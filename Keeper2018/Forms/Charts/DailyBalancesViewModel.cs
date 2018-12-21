using System;
using System.Collections.Generic;
using System.Threading;
using Caliburn.Micro;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Keeper2018
{
    public class DailyBalancesViewModel : Screen
    {
        private KeeperDb _db;
        public PlotModel DailyBalancesModel { get; set; }

        public void Initialize(KeeperDb db)
        {
            _db = db;
            DailyBalancesModel = new PlotModel();
           // var series = PrepareSeries();
            var series = GetSeries(GetAmounts(GetBalances()));
            DailyBalancesModel.Series.Add(series);

            DailyBalancesModel.Axes.Add(new DateTimeAxis()
            {
                Position = AxisPosition.Bottom,
                IntervalLength = 75,
                MinorIntervalType = DateTimeIntervalType.Days,
                IntervalType = DateTimeIntervalType.Days,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dash,
            });
        }

        private Balance _balance = new Balance();
        private LineSeries PrepareSeries()
        {
            var result = new LineSeries();
            var currentDate = new DateTime(2001, 12, 31);
            foreach (var tran in _db.TransactionModels)
            {
                if (!tran.Timestamp.Date.Equals(currentDate))
                {
                    var day = DateTimeAxis.ToDouble(currentDate);
                    result.Points.Add(new DataPoint(day, (double)_db.BalanceInUsd(currentDate, _balance)));

                    currentDate = currentDate.AddDays(1);
                }
                RegisterTran(tran);
            }
            return result;
        }

        private LineSeries GetSeries(List<double> values)
        {
            var result = new LineSeries();
            var currentDate = new DateTime(2001, 12, 31);
            foreach (var value in values)
            {
                var day = DateTimeAxis.ToDouble(currentDate);
                result.Points.Add(new DataPoint(day, value));

                currentDate = currentDate.AddDays(1);
            }
            return result;
        }
        private List<double> GetAmounts(List<Balance> balances)
        {
            var result = new List<double>();
            var currentDate = new DateTime(2001, 12, 31);
            foreach (var balance in balances)
            {
                result.Add((double)_db.BalanceInUsd(currentDate, balance));
                currentDate = currentDate.AddDays(1);
            }
            return result;
        }

        private List<Balance> GetBalances()
        {
            var result = new List<Balance>();
            var currentDate = new DateTime(2001, 12, 31);
            foreach (var tran in _db.TransactionModels)
            {
                if (!tran.Timestamp.Date.Equals(currentDate))
                {
                    result.Add(_balance);
                    currentDate = currentDate.AddDays(1);
                }
                RegisterTran(tran);
            }
            return result;
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
