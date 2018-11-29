using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public class TrafficOfAccountCalculator : ITraffic
    {
        private readonly AccountModel _accountModel;
        private readonly Period _period;
        private readonly KeeperDb _db;
        private readonly TrafficOfAccount _traffic = new TrafficOfAccount();
        public List<string> AccountReport;
        private readonly List<string> _shortTrans = new List<string>();
        private readonly List<ReportLine> _reportLines = new List<ReportLine>();
        private readonly bool _isDeposit;
        private BalanceOfAccount _balanceOfAccount = new BalanceOfAccount();

        public decimal AmountInUsd => _db.BalanceInUsd(_period.FinishMoment, _traffic.Balance());
        public string Total => _db.BalanceInUsd(_period.FinishMoment, _traffic.Balance()).ToString("0.## usd");

        public TrafficOfAccountCalculator(KeeperDb db, AccountModel accountModel, Period period)
        {
            _accountModel = accountModel;
            _period = period;
            _isDeposit = _accountModel.Deposit != null;
            _db = db;
        }

        public void Evaluate()
        {
            foreach (var tran in _db.TransactionModels.Where(t => _period.Includes(t.Timestamp)))
                RegisterTran(tran);

        }

        public void RegisterTran(TransactionModel tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход:
                    if (tran.MyAccount.Equals(_accountModel))
                    {
                        _traffic.Add(tran.Currency, tran.Amount);
                        _shortTrans.Add(_db.ShortLine(tran, false, 1));
                        if (_isDeposit)
                            _reportLines.Add(_db.ReportLine(_balanceOfAccount, tran, false, 1));
                    }
                    break;
                case OperationType.Расход:
                    if (tran.MyAccount.Equals(_accountModel))
                    {
                        _traffic.Sub(tran.Currency, tran.Amount);
                        _shortTrans.Add(_db.ShortLine(tran, false, -1));
                        if (_isDeposit)
                            _reportLines.Add(_db.ReportLine(_balanceOfAccount, tran, false, -1));
                    }
                    break;
                case OperationType.Перенос:
                    if (tran.MyAccount.Equals(_accountModel))
                    {
                        _traffic.Sub(tran.Currency, tran.Amount);
                        _shortTrans.Add(_db.ShortLine(tran, false, -1));
                        if (_isDeposit)
                            _reportLines.Add(_db.ReportLine(_balanceOfAccount, tran, false, -1));
                    }
                    if (tran.MySecondAccount.Equals(_accountModel))
                    {
                        _traffic.Add(tran.Currency, tran.Amount);
                        _shortTrans.Add(_db.ShortLine(tran, false, 1));
                        if (_isDeposit)
                            _reportLines.Add(_db.ReportLine(_balanceOfAccount, tran, true, 1));
                    }
                    break;
                case OperationType.Обмен:
                    // явочный обмен - приходишь в банк и меняешь - была в кармане одна валюта - стала другая в том же кармане
                    if (tran.MyAccount.Equals(_accountModel) && tran.MySecondAccount.Equals(_accountModel))
                    {
                        _traffic.Sub(tran.Currency, tran.Amount);
                        // ReSharper disable once PossibleInvalidOperationException
                        _traffic.Add((CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                        _shortTrans.Add(_db.ShortLineOneAccountExchange(tran));
                        if (_isDeposit)
                            _reportLines.Add(_db.ReportLineOneAccountExchange(_balanceOfAccount, tran));
                    }
                    // безнальный обмен - с одного счета списалась одна валюта, на другой зачислилась другая
                    else
                    {
                        if (tran.MyAccount.Equals(_accountModel))
                        {
                            _traffic.Sub(tran.Currency, tran.Amount);
                            _shortTrans.Add(_db.ShortLine(tran, false, -1));
                            if (_isDeposit)
                                _reportLines.Add(_db.ReportLine(_balanceOfAccount, tran, false, -1));
                        }

                        if (tran.MySecondAccount.Equals(_accountModel))
                        {
                            // ReSharper disable once PossibleInvalidOperationException
                            _traffic.Add((CurrencyCode) tran.CurrencyInReturn, tran.AmountInReturn);
                            _shortTrans.Add(_db.ShortLine(tran, true, 1));
                            if (_isDeposit)
                                _reportLines.Add(_db.ReportLine(_balanceOfAccount, tran, true, 1));
                        }
                    }
                    break;
            }
        }

        public IEnumerable<string> Report(BalanceOrTraffic mode)
        {
            foreach (var line in _traffic.Report(mode))
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