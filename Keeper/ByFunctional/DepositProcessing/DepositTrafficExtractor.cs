using System.Composition;
using System.Collections.Generic;
using System.Linq;
using Keeper.DomainModel;
using Keeper.DomainModel.Deposit;
using Keeper.DomainModel.Transactions;
using Keeper.Utils.Rates;

namespace Keeper.ByFunctional.DepositProcessing
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
            ExtractViaSqlRequest();  
            return _deposit;
        }

        private void ExtractViaSqlRequest()
        {
            _deposit.CalculationData.Traffic = 
                (from t in _db.Transactions
                 where t.Debet == _deposit.ParentAccount || t.Credit == _deposit.ParentAccount
                 orderby t.Timestamp
                 join r in _db.CurrencyRates on new { t.Timestamp.Date, t.Currency } equals new { r.BankDay.Date, r.Currency } into g
                 from rate in g.DefaultIfEmpty()
                 select new DepositTransaction
                 {
                     Amount = t.Amount,
                     Timestamp = t.Timestamp,
                     Currency = t.Currency,
                     Counteragent = GetDepositCounteragent(t),
                     Comment = GetDepositOperationComment(t),
                     AmountInUsd = rate != null ? t.Amount / (decimal)rate.Rate : t.Amount,
                     TransactionType = GetDepositOperationType(t, _deposit.ParentAccount)
                 }).ToList();
        }

        private DepositTransactionTypes GetDepositOperationType(Transaction t, Account depositAccount)
        {
            return (t.Operation == OperationType.Доход && !t.IsExchange())
                     ? DepositTransactionTypes.Проценты
                     : t.Debet == depositAccount
                         ? DepositTransactionTypes.Расход
                         : DepositTransactionTypes.Явнес;
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
