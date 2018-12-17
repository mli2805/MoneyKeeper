using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public class TrafficOfTagBranchCalculator : ITraffic
    {
        private readonly KeeperDb _db;
        private readonly AccountModel _tag;
        private readonly Period _period;

        private readonly BalanceWithTurnoverOfBranch _balanceWithTurnovers = new BalanceWithTurnoverOfBranch();
        public string Total => "";

        public TrafficOfTagBranchCalculator(KeeperDb db, AccountModel tag, Period period)
        {
            _db = db;
            _tag = tag;
            _period = period;
        }

        public void Evaluate()
        {
            foreach (var tran in _db.TransactionModels.Where(t => _period.Includes(t.Timestamp)))
            {
                foreach (var tag in tran.Tags)
                {
                    var myTag = tag.IsC(_tag);
                    if (myTag != null)
                        RegisterTran(tran, myTag);
                }
            }
        }

        private void RegisterTran(TransactionModel tran, AccountModel myTag)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход:
                    _balanceWithTurnovers.Add(myTag, tran.Currency, tran.Amount);
                    break;
                case OperationType.Расход:
                    _balanceWithTurnovers.Sub(myTag, tran.Currency, tran.Amount);
                    break;
                case OperationType.Перенос:
                    _balanceWithTurnovers.Add(myTag, tran.Currency, tran.Amount);
                    _balanceWithTurnovers.Sub(myTag, tran.Currency, tran.Amount);
                    break;
                case OperationType.Обмен:
                    _balanceWithTurnovers.Add(myTag, tran.Currency, tran.Amount);
                    // ReSharper disable once PossibleInvalidOperationException
                    _balanceWithTurnovers.Sub(myTag, (CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                    break;
            }
        }

        public IEnumerable<string> Report(BalanceOrTraffic mode) { return _balanceWithTurnovers.Report(mode); }
    }
}