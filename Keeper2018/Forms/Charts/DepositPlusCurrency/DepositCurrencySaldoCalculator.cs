using System;
using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public class DepositCurrencySaldoCalculator
    {
        private readonly KeeperDataModel _dataModel;
        private Balance _initialBalance;
        private DateTime _startDate = new DateTime(2002, 1, 1);

        public DepositCurrencySaldoCalculator(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
            _initialBalance = new Balance();
            _initialBalance.Add(CurrencyCode.USD, 1022);
            _initialBalance.Add(CurrencyCode.BYR, 32500);
        }

        public List<DepoCurrencyData> Evaluate()
        {
            var date = _startDate;
            var balance = _initialBalance;
            var result = new List<DepoCurrencyData>();

            while (date < DateTime.Now)
            {
                var period = new Period(date, date.GetEndOfMonth());
                var point = GetBalanceAndIncome(balance, period);
                result.Add(point);

                date = date.AddMonths(1);
            }

            return result;
        }

        private DepoCurrencyData GetBalanceAndIncome(Balance balance, Period period)
        {
            decimal before = balance.ToUsd(_dataModel, period.StartDate.AddDays(-1));

            decimal incomeUsd = 0;
            decimal depositIncomeUsd = 0;
            decimal expensesUsd = 0;

            var trans = _dataModel.Transactions.Where(t => period.Includes(t.Value.Timestamp)).ToList();
            foreach (var d in trans)
            {
                var tran = d.Value;
                switch (tran.Operation)
                {
                    case OperationType.Доход :
                        var inUsd = _dataModel.AmountInUsd(tran.Timestamp, tran.Currency, tran.Amount);
                        incomeUsd += inUsd;
                        if (tran.Tags.Contains(_dataModel.PercentsTag()))
                            depositIncomeUsd += inUsd;
                        balance.Add(tran.Currency, tran.Amount);
                        break;
                    case OperationType.Расход :
                        expensesUsd += _dataModel.AmountInUsd(tran.Timestamp, tran.Currency, tran.Amount);
                        balance.Sub(tran.Currency, tran.Amount);
                        break;
                    case OperationType.Обмен :
                        balance.Sub(tran.Currency, tran.Amount);
                        if (tran.CurrencyInReturn != null)
                            balance.Add((CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                        break;
                }
            }

            decimal after = balance.ToUsd(_dataModel, period.FinishMoment.Date);

            var oneMonth = new DepoCurrencyData()
            {
                StartDate = period.StartDate,
                DepoRevenue = depositIncomeUsd,
                CurrencyRatesDifferrence = after - (before + incomeUsd - expensesUsd),
            };

            return oneMonth;
        }
    }
}