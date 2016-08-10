using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Deposit;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Trans;

namespace Keeper.Utils.DepositProcessing
{
    [Export]
    public class DepositTrafficExtractor
    {
        private readonly KeeperDb _db;
        private Deposit _deposit;

        [ImportingConstructor]
        public DepositTrafficExtractor(KeeperDb db)
        {
            _db = db;
        }

        public Deposit ExtractTraffic(Account account) // используется при месячном анализе и выгрузке в excel
        {
            _deposit = account.Deposit;
            _deposit.CalculationData = new DepositCalculationData();
            var temp = Extract();
            temp.AddRange(ExtractSecondPart());
            _deposit.CalculationData.Traffic = temp.OrderBy(t => t.Timestamp).ToList();
            return _deposit;
        }

        private List<DepositTransaction> Extract()
        {
            return (from t in _db.TransWithTags
                    where 
                        t.MyAccount == _deposit.ParentAccount 
                    orderby t.Timestamp
                    join r in _db.CurrencyRates on new { t.Timestamp.Date, Currency = t.Currency.GetValueOrDefault() } equals new { r.BankDay.Date, r.Currency } into g
                    from rate in g.DefaultIfEmpty()
                    select new DepositTransaction
                    {
                        Amount = t.Amount,
                        Timestamp = t.Timestamp,
                        Currency = t.Currency.GetValueOrDefault(),
                        Counteragent = GetDepositCounteragent(t),
                        Comment = t.Comment != "" ? t.Comment : t.Operation == OperationType.Обмен ? "снятие с обменом валюты" : t.Operation == OperationType.Доход ? "проценты" : "частичное снятие",
                        AmountInUsd = rate != null ? t.Amount / (decimal)rate.Rate : t.Amount,
                        TransactionType = GetDepositOperationType(t, _deposit.ParentAccount)
                    }).ToList();
        }

        private IEnumerable<DepositTransaction> ExtractSecondPart()
        {
            return from t in _db.TransWithTags
                   where
                       (t.MySecondAccount != null && t.MySecondAccount == _deposit.ParentAccount)
                   orderby t.Timestamp
                   join r in _db.CurrencyRates on new { t.Timestamp.Date, Currency = t.CurrencyInReturn.GetValueOrDefault() } equals
                       new { r.BankDay.Date, r.Currency } into g
                   from rate in g.DefaultIfEmpty()
                   select new DepositTransaction
                   {
                       Amount = t.Operation == OperationType.Обмен ? t.AmountInReturn : t.Amount,
                       Timestamp = t.Timestamp,
                       Currency = t.CurrencyInReturn.GetValueOrDefault(),
                       Counteragent = GetDepositCounteragent(t),
                       Comment = t.Operation == OperationType.Обмен ? "обмен с пополнением депозита" : "пополнение депозита",
                       AmountInUsd = rate != null ? t.AmountInReturn / (decimal)rate.Rate : t.AmountInReturn,
                       TransactionType = DepositTransactionTypes.Явнес,
                   };
        }

        private DepositTransactionTypes GetDepositOperationType(TranWithTags t, Account depositAccount)
        {
            return t.Operation == OperationType.Доход
                     ? DepositTransactionTypes.Проценты
                     : t.MyAccount == depositAccount
                         ? DepositTransactionTypes.Расход
                         : DepositTransactionTypes.Явнес;
        }

        private string GetDepositCounteragent(TranWithTags t)
        {
            return t.Operation == OperationType.Доход ?
                "банк" :
                t.MyAccount == _deposit.ParentAccount ?
                         t.MySecondAccount == null ? "" : t.MySecondAccount.Name
                         : t.MyAccount.Name;
        }
    }
}
