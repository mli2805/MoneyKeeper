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
            //            ExtractViaSqlRequest();
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
//                        || (t.MySecondAccount != null && t.MySecondAccount == _deposit.ParentAccount)
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
//                       t.MyAccount == _deposit.ParentAccount &&
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
//                       Comment = "смена валюты депозита",
                       Comment = t.Operation == OperationType.Обмен ? "обмен с пополнением депозита" : "пополнение депозита",
                       AmountInUsd = rate != null ? t.AmountInReturn / (decimal)rate.Rate : t.AmountInReturn,
                       TransactionType = DepositTransactionTypes.Явнес,
                   };
        }

        //        private void ExtractViaSqlRequest()
        //        {
        //            _deposit.CalculationData.Traffic =
        //                (from t in _db.Transactions
        //                 where t.Debet == _deposit.ParentAccount || t.Credit == _deposit.ParentAccount
        //                 orderby t.Timestamp
        //                 join r in _db.CurrencyRates on new { t.Timestamp.Date, t.Currency } equals new { r.BankDay.Date, r.Currency } into g
        //                 from rate in g.DefaultIfEmpty()
        //                 select new DepositTransaction
        //                 {
        //                     Amount = t.Amount,
        //                     Timestamp = t.Timestamp,
        //                     Currency = t.Currency,
        //                     Counteragent = GetDepositCounteragent(t),
        //                     Comment = GetDepositOperationComment(t),
        //                     AmountInUsd = rate != null ? t.Amount / (decimal)rate.Rate : t.Amount,
        //                     TransactionType = GetDepositOperationType(t, _deposit.ParentAccount)
        //                 }).ToList();
        //        }

        private DepositTransactionTypes GetDepositOperationType(TranWithTags t, Account depositAccount)
        {
            return t.Operation == OperationType.Доход
                     ? DepositTransactionTypes.Проценты
                     : t.MyAccount == depositAccount
                         ? DepositTransactionTypes.Расход
                         : DepositTransactionTypes.Явнес;
        }

        private DepositTransactionTypes GetDepositOperationType(Transaction t, Account depositAccount)
        {
            return (t.Operation == OperationType.Доход && !t.IsExchange())
                     ? DepositTransactionTypes.Проценты
                     : t.Debet == depositAccount
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
        private string GetDepositCounteragent(Transaction t)
        {
            return t.Debet == _deposit.ParentAccount ? t.Credit.Name : t.Debet.Name;
        }


        private string GetDepositOperationComment(Transaction t)
        {
            if (t.Comment != "") return t.Comment;
            if (t.Article != null) return t.Article.Name.ToLower();
            if (t.Credit.Is("Мой кошелек")) return "снял наличными";
            if (t.Debet.Is("БИБ Зарплатная GOLD")) return "перекинул с зарплатной";
            return "";
        }

    }
}
