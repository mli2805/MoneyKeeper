using System;
using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public class TrafficOfAccountCalculator : ITraffic
    {
        private readonly AccountItemModel _accountItemModel;
        private readonly Period _period;
        private readonly KeeperDataModel _dataModel;
        private readonly BalanceWithTurnover _balanceWithTurnover = new BalanceWithTurnover();
        private readonly SortedDictionary<DateTime, ListLine> _coloredTrans = new SortedDictionary<DateTime, ListLine>();

        private readonly bool _isDeposit;
        public DepositReportModel DepositReportModel { get; set; }

        private decimal AmountInUsd => _dataModel.BalanceInUsd(_period.FinishMoment, _balanceWithTurnover.Balance());
        

        public string Total => AmountInUsd.ToString("0.## usd");

        public TrafficOfAccountCalculator(KeeperDataModel dataModel, AccountItemModel accountItemModel, Period period)
        {
            _accountItemModel = accountItemModel;
            _period = period;
            _isDeposit = _accountItemModel.IsDeposit;

            _dataModel = dataModel;

            DepositReportModel = new DepositReportModel(dataModel);
        }

        public bool TryGetValue(CurrencyCode mainCurrency, out decimal result)
        {
            if (_balanceWithTurnover.Currencies.TryGetValue(mainCurrency, out TrafficPair trafficPair))
            {
                result = trafficPair.Plus - trafficPair.Minus;
                return true;
            }

            result = 0;
            return false;

        }

        public void EvaluateAccount()
        {
            Evaluate();

            if (_isDeposit)
            {
                DepositReportModel.BankAccount = _accountItemModel.BankAccount;
                DepositReportModel.Balance = _balanceWithTurnover.Balance();
                DepositReportModel.AmountInUsd = AmountInUsd;
                DepositReportModel.DepositName = _accountItemModel.Name;
            }
        }

        public Balance EvaluateBalance()
        {
            Evaluate();
            return _balanceWithTurnover.Balance();
        }

        private void Evaluate()
        {
            foreach (var tran in _dataModel.Transactions.Values.Where(t => _period.Includes(t.Timestamp)))
            {
                switch (tran.Operation)
                {
                    case OperationType.Доход:
                        RegisterIncome(tran);
                        break;
                    case OperationType.Расход:
                        RegisterOutcome(tran);
                        break;
                    case OperationType.Перенос:
                        RegisterTransfer(tran);
                        break;
                    case OperationType.Обмен:
                        RegisterExchange(tran);
                        break;
                }
            }
        }


        private void RegisterIncome(TransactionModel tran)
        {
            if (tran.MyAccount.Id != _accountItemModel.Id) return;

            _coloredTrans.Add(tran.Timestamp, _dataModel.ColoredLine(tran, false, 1));
            DepositReportModel.Traffic.Add(_dataModel.ReportLine(_balanceWithTurnover.Balance(), tran, false, 1, DepositOperationType.Revenue));
            _balanceWithTurnover.Add(tran.Currency, tran.Amount);
        }

        private void RegisterOutcome(TransactionModel tran)
        {
            if (tran.MyAccount.Id != _accountItemModel.Id) return;

            _coloredTrans.Add(tran.Timestamp, _dataModel.ColoredLine(tran, false, -1));
            if (_isDeposit)
                DepositReportModel.Traffic.Add(_dataModel.ReportLine(_balanceWithTurnover.Balance(), tran, false, -1, DepositOperationType.Consumption));
            _balanceWithTurnover.Sub(tran.Currency, tran.Amount);
        }

        private void RegisterTransfer(TransactionModel tran)
        {
            if (tran.MyAccount.Id == _accountItemModel.Id)
            {
                _coloredTrans.Add(tran.Timestamp, _dataModel.ColoredLine(tran, false, -1));
                if (_isDeposit)
                    DepositReportModel.Traffic.Add(_dataModel.ReportLine(_balanceWithTurnover.Balance(), tran, false, -1, DepositOperationType.Consumption));
                _balanceWithTurnover.Sub(tran.Currency, tran.Amount);
            }

            if (tran.MySecondAccount.Id ==_accountItemModel.Id)
            {
                _coloredTrans.Add(tran.Timestamp, _dataModel.ColoredLine(tran, false, 1));
                if (_isDeposit)
                    DepositReportModel.Traffic.Add(_dataModel.ReportLine(_balanceWithTurnover.Balance(), tran, false, 1, DepositOperationType.Contribution));
                _balanceWithTurnover.Add(tran.Currency, tran.Amount);
            }
        }

        private void RegisterExchange(TransactionModel tran)
        {
            // явочный обмен - приходишь в банк и меняешь - была в кармане одна валюта - стала другая в том же кармане
            if (tran.MyAccount.Id == _accountItemModel.Id && tran.MySecondAccount.Id == _accountItemModel.Id)
            {
                _coloredTrans.Add(tran.Timestamp, _dataModel.ColoredLineOneAccountExchange(tran));
                if (_isDeposit)
                    DepositReportModel.Traffic.Add(_dataModel.ReportLineOneAccountExchange(_balanceWithTurnover.Balance(), tran));
                _balanceWithTurnover.Sub(tran.Currency, tran.Amount);
                // ReSharper disable once PossibleInvalidOperationException
                _balanceWithTurnover.Add((CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
            }
            // безнальный обмен - с одного счета списалась одна валюта, на другой зачислилась другая
            else
            {
                if (tran.MyAccount.Id == _accountItemModel.Id)
                {
                    _coloredTrans.Add(tran.Timestamp, _dataModel.ColoredLine(tran, false, -1));
                    if (_isDeposit)
                        DepositReportModel.Traffic.Add(_dataModel.ReportLine(_balanceWithTurnover.Balance(), tran, false, -1, DepositOperationType.Consumption));
                    _balanceWithTurnover.Sub(tran.Currency, tran.Amount);
                }

                if (tran.MySecondAccount.Id == _accountItemModel.Id)
                {
                    _coloredTrans.Add(tran.Timestamp, _dataModel.ColoredLine(tran, true, 1));
                    if (_isDeposit)
                    {
                        DepositReportModel.Traffic.Add(_dataModel.ReportLine(_balanceWithTurnover.Balance(), tran, true, 1, DepositOperationType.Contribution));
                    }
                    // ReSharper disable once PossibleInvalidOperationException
                    _balanceWithTurnover.Add((CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                }
            }
        }

        public IEnumerable<KeyValuePair<DateTime, ListLine>> ColoredReport(BalanceOrTraffic mode)
        {
            foreach (var line in _balanceWithTurnover.ColoredReport(mode))
            {
                yield return line;
            }
            foreach (var pair in _coloredTrans.Reverse())
            {
                yield return pair;
            }
        }

    }
}