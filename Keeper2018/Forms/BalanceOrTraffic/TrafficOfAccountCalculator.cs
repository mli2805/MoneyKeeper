using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public class TrafficOfAccountCalculator : ITraffic
    {
        private readonly AccountModel _accountModel;
        private readonly Period _period;
        private readonly KeeperDb _db;
        private readonly BalanceWithTurnover _balanceWithTurnover = new BalanceWithTurnover();
        private readonly List<string> _shortTrans = new List<string>();

        private readonly bool _isDeposit;
        public DepositReportModel DepositReportModel { get; set; }

        public decimal AmountInUsd => _db.BalanceInUsd(_period.FinishMoment, _balanceWithTurnover.Balance());
        public string Total => AmountInUsd.ToString("0.## usd");

        public TrafficOfAccountCalculator(KeeperDb db, AccountModel accountModel, Period period)
        {
            _accountModel = accountModel;
            _period = period;
            _isDeposit = _accountModel.Deposit != null;
            _db = db;

            DepositReportModel = new DepositReportModel(db);
        }

        public void Evaluate()
        {
            foreach (var tran in _db.TransactionModels.Where(t => _period.Includes(t.Timestamp)))
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

            if (_isDeposit)
            {
                DepositReportModel.DepositName = _accountModel.Name;
                DepositReportModel.DepositState = AmountInUsd == 0 ? "Депозит закрыт." : "Действующий депозит.";
                DepositReportModel.Balance = _balanceWithTurnover.Balance();
            }
        }

        private void RegisterIncome(TransactionModel tran)
        {
            if (!tran.MyAccount.Equals(_accountModel)) return;

            _shortTrans.Add(_db.ShortLine(tran, false, 1));
            if (_isDeposit)
            {
                DepositReportModel.Traffic.Add(_db.ReportLine(_balanceWithTurnover.Balance(), tran, false, 1));
                DepositReportModel.Revenue.Add(tran.Currency, tran.Amount);
                DepositReportModel.RevenueUsd += _db.AmountInUsd(tran.Timestamp, tran.Currency, tran.Amount);
            }
            _balanceWithTurnover.Add(tran.Currency, tran.Amount);
        }

        private void RegisterOutcome(TransactionModel tran)
        {
            if (!tran.MyAccount.Equals(_accountModel)) return;

            _shortTrans.Add(_db.ShortLine(tran, false, -1));
            if (_isDeposit)
            {
                DepositReportModel.Traffic.Add(_db.ReportLine(_balanceWithTurnover.Balance(), tran, false, -1));
                DepositReportModel.Сonsumption.Add(tran.Currency, tran.Amount); // payment from deposit directly (by card which is deposit)
                DepositReportModel.ConsumptionUsd += _db.AmountInUsd(tran.Timestamp, tran.Currency, tran.Amount);
            }
            _balanceWithTurnover.Sub(tran.Currency, tran.Amount);
        }

        private void RegisterTransfer(TransactionModel tran)
        {
            if (tran.MyAccount.Equals(_accountModel))
            {
                _shortTrans.Add(_db.ShortLine(tran, false, -1));
                if (_isDeposit)
                {
                    DepositReportModel.Traffic.Add(_db.ReportLine(_balanceWithTurnover.Balance(), tran, false, -1));
                    DepositReportModel.Сonsumption.Add(tran.Currency, tran.Amount);
                    DepositReportModel.ConsumptionUsd += _db.AmountInUsd(tran.Timestamp, tran.Currency, tran.Amount);
                }

                _balanceWithTurnover.Sub(tran.Currency, tran.Amount);
            }

            if (tran.MySecondAccount.Equals(_accountModel))
            {
                _shortTrans.Add(_db.ShortLine(tran, false, 1));
                if (_isDeposit)
                {
                    DepositReportModel.Traffic.Add(_db.ReportLine(_balanceWithTurnover.Balance(), tran, false, 1));
                    DepositReportModel.Contribution.Add(tran.Currency, tran.Amount);
                    DepositReportModel.ContributionUsd += _db.AmountInUsd(tran.Timestamp, tran.Currency, tran.Amount);
                }

                _balanceWithTurnover.Add(tran.Currency, tran.Amount);
            }
        }

        private void RegisterExchange(TransactionModel tran)
        {
            // явочный обмен - приходишь в банк и меняешь - была в кармане одна валюта - стала другая в том же кармане
            if (tran.MyAccount.Equals(_accountModel) && tran.MySecondAccount.Equals(_accountModel))
            {
                _shortTrans.Add(_db.ShortLineOneAccountExchange(tran));
                if (_isDeposit)
                {
                    DepositReportModel.Traffic.Add(_db.ReportLineOneAccountExchange(_balanceWithTurnover.Balance(), tran));
                }
                _balanceWithTurnover.Sub(tran.Currency, tran.Amount);
                // ReSharper disable once PossibleInvalidOperationException
                _balanceWithTurnover.Add((CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
            }
            // безнальный обмен - с одного счета списалась одна валюта, на другой зачислилась другая
            else
            {
                if (tran.MyAccount.Equals(_accountModel))
                {
                    _shortTrans.Add(_db.ShortLine(tran, false, -1));
                    if (_isDeposit)
                    {
                        DepositReportModel.Traffic.Add(_db.ReportLine(_balanceWithTurnover.Balance(), tran, false, -1));
                        DepositReportModel.Сonsumption.Add(tran.Currency, tran.Amount);
                        DepositReportModel.ConsumptionUsd += _db.AmountInUsd(tran.Timestamp, tran.Currency, tran.Amount);
                    }
                    _balanceWithTurnover.Sub(tran.Currency, tran.Amount);
                }

                if (tran.MySecondAccount.Equals(_accountModel))
                {
                    _shortTrans.Add(_db.ShortLine(tran, true, 1));
                    if (_isDeposit)
                    {
                        DepositReportModel.Traffic.Add(_db.ReportLine(_balanceWithTurnover.Balance(), tran, true, 1));
                        // ReSharper disable once PossibleInvalidOperationException
                        DepositReportModel.Contribution.Add((CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                        DepositReportModel.ContributionUsd += _db.AmountInUsd(tran.Timestamp, (CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                    }
                    // ReSharper disable once PossibleInvalidOperationException
                    _balanceWithTurnover.Add((CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                }
            }
        }

        public IEnumerable<string> Report(BalanceOrTraffic mode)
        {
            foreach (var line in _balanceWithTurnover.Report(mode))
            {
                yield return line;
            }
            foreach (var tran in _shortTrans)
            {
                yield return tran;
            }
        }
    }
}