using System;
using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public class TrafficOfAccountCalculator : ITraffic
    {
        private readonly AccountModel _accountModel;
        private readonly Period _period;
        private readonly KeeperDataModel _dataModel;
        private readonly BalanceWithTurnover _balanceWithTurnover = new BalanceWithTurnover();
        private readonly SortedDictionary<DateTime, string> _shortTrans = new SortedDictionary<DateTime, string>();

        private readonly bool _isDeposit;
        public DepositReportModel DepositReportModel { get; set; }

        private decimal AmountInUsd => _dataModel.BalanceInUsd(_period.FinishMoment, _balanceWithTurnover.Balance());
        public string Total => AmountInUsd.ToString("0.## usd");

        public TrafficOfAccountCalculator(KeeperDataModel dataModel, AccountModel accountModel, Period period)
        {
            _accountModel = accountModel;
            _period = period;
            _isDeposit = _accountModel.Deposit != null;
            _dataModel = dataModel;

            DepositReportModel = new DepositReportModel(dataModel);
        }

        public void EvaluateAccount()
        {
            Evaluate();

            if (_isDeposit)
            {
                DepositReportModel.Deposit = _accountModel.Deposit;
                DepositReportModel.Balance = _balanceWithTurnover.Balance();
                DepositReportModel.AmountInUsd = AmountInUsd;
                DepositReportModel.DepositName = _accountModel.Name;
            }
        }

        public Balance EvaluateBalance()
        {
            Evaluate();
            return _balanceWithTurnover.Balance();
        }

        private void Evaluate()
        {
            foreach (var tran in _dataModel.Bin.Transactions.Values.Where(t => _period.Includes(t.Timestamp)))
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


        private void RegisterIncome(Transaction tran)
        {
            if (tran.MyAccount != _accountModel.Id) return;

            _shortTrans.Add(tran.Timestamp, _dataModel.ShortLine(tran, false, 1));
            DepositReportModel.Traffic.Add(_dataModel.ReportLine(_balanceWithTurnover.Balance(), tran, false, 1, DepositOperationType.Revenue));
            _balanceWithTurnover.Add(tran.Currency, tran.Amount);
        }

        private void RegisterOutcome(Transaction tran)
        {
            if (tran.MyAccount != _accountModel.Id) return;

            _shortTrans.Add(tran.Timestamp, _dataModel.ShortLine(tran, false, -1));
            if (_isDeposit)
                DepositReportModel.Traffic.Add(_dataModel.ReportLine(_balanceWithTurnover.Balance(), tran, false, -1, DepositOperationType.Consumption));
            _balanceWithTurnover.Sub(tran.Currency, tran.Amount);
        }

        private void RegisterTransfer(Transaction tran)
        {
            if (tran.MyAccount == _accountModel.Id)
            {
                _shortTrans.Add(tran.Timestamp, _dataModel.ShortLine(tran, false, -1));
                if (_isDeposit)
                    DepositReportModel.Traffic.Add(_dataModel.ReportLine(_balanceWithTurnover.Balance(), tran, false, -1, DepositOperationType.Consumption));
                _balanceWithTurnover.Sub(tran.Currency, tran.Amount);
            }

            if (tran.MySecondAccount ==_accountModel.Id)
            {
                _shortTrans.Add(tran.Timestamp, _dataModel.ShortLine(tran, false, 1));
                if (_isDeposit)
                    DepositReportModel.Traffic.Add(_dataModel.ReportLine(_balanceWithTurnover.Balance(), tran, false, 1, DepositOperationType.Contribution));
                _balanceWithTurnover.Add(tran.Currency, tran.Amount);
            }
        }

        private void RegisterExchange(Transaction tran)
        {
            // явочный обмен - приходишь в банк и меняешь - была в кармане одна валюта - стала другая в том же кармане
            if (tran.MyAccount == _accountModel.Id && tran.MySecondAccount == _accountModel.Id)
            {
                _shortTrans.Add(tran.Timestamp, _dataModel.ShortLineOneAccountExchange(tran));
                if (_isDeposit)
                    DepositReportModel.Traffic.Add(_dataModel.ReportLineOneAccountExchange(_balanceWithTurnover.Balance(), tran));
                _balanceWithTurnover.Sub(tran.Currency, tran.Amount);
                // ReSharper disable once PossibleInvalidOperationException
                _balanceWithTurnover.Add((CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
            }
            // безнальный обмен - с одного счета списалась одна валюта, на другой зачислилась другая
            else
            {
                if (tran.MyAccount == _accountModel.Id)
                {
                    _shortTrans.Add(tran.Timestamp, _dataModel.ShortLine(tran, false, -1));
                    if (_isDeposit)
                        DepositReportModel.Traffic.Add(_dataModel.ReportLine(_balanceWithTurnover.Balance(), tran, false, -1, DepositOperationType.Consumption));
                    _balanceWithTurnover.Sub(tran.Currency, tran.Amount);
                }

                if (tran.MySecondAccount == _accountModel.Id)
                {
                    _shortTrans.Add(tran.Timestamp, _dataModel.ShortLine(tran, true, 1));
                    if (_isDeposit)
                    {
                        DepositReportModel.Traffic.Add(_dataModel.ReportLine(_balanceWithTurnover.Balance(), tran, true, 1, DepositOperationType.Contribution));
                        // ReSharper disable once PossibleInvalidOperationException
                        DepositReportModel.Contribution.Add((CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                        DepositReportModel.ContributionUsd += _dataModel.AmountInUsd(tran.Timestamp, (CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                    }
                    // ReSharper disable once PossibleInvalidOperationException
                    _balanceWithTurnover.Add((CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                }
            }
        }

        public IEnumerable<KeyValuePair<DateTime, string>> Report(BalanceOrTraffic mode)
        {
            foreach (var pair in _balanceWithTurnover.Report(mode))
            {
                yield return pair;
            }
            foreach (var pair in _shortTrans.Reverse())
            {
                yield return pair;
            }
        }
    }
}