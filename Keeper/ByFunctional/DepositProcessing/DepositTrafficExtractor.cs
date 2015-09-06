using System.Composition;
using System.Collections.Generic;
using System.Linq;
using Keeper.DomainModel;
using Keeper.DomainModel.Deposit;
using Keeper.Utils.Rates;

namespace Keeper.ByFunctional.DepositProcessing
{
    [Export]
    public class DepositTrafficExtractor
    {
        private readonly KeeperDb _db;
        private Deposit _deposit;
        private RateExtractor _rateExtractor;

        [ImportingConstructor]
        public DepositTrafficExtractor(KeeperDb db, RateExtractor rateExtractor)
        {
            _db = db;
            _rateExtractor = rateExtractor;
        }

        public Deposit ExtractTraffic(Account account) // используется при месячном анализе
        {
            _deposit = account.Deposit;
            _deposit.CalculationData = new DepositCalculationData();
            ExtractViaSqlRequest();   // быстрый
//            ExtractInCombainedMode();
            return _deposit;
        }

        private void ExtractViaSqlRequest() // проблемы с операциями обмена (должны отобразиться в обе стороны)
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
                     Comment = GetDepositOperationComment(t),
                     AmountInUsd = rate != null ? t.Amount / (decimal)rate.Rate : t.Amount,
                     TransactionType = GetDepositOperationType(t, _deposit.ParentAccount)
                 }).ToList();
        }

        private void ExtractInCombainedMode()
        {
            var trs = from t in _db.Transactions
                      where t.Debet == _deposit.ParentAccount || t.Credit == _deposit.ParentAccount
                      select t;

            _deposit.CalculationData.Traffic = new List<DepositTransaction>();
            foreach (var tr in trs) 
            {
//                if (tr.Operation == OperationType.Обмен)
//                    _deposit.CalculationData.Traffic.AddRange(ConvertExchangeTransactionToDepositTransaction(tr));
//                else
//                    _deposit.CalculationData.Traffic.Add(ConvertCommonTransactionToDepositTransaction(tr));

            }
        }

        private DepositTransaction ConvertCommonTransactionToDepositTransaction(Transaction t)
        {
            var result = new DepositTransaction()
                {
                    Amount = t.Amount,
                    Timestamp = t.Timestamp,
                    Currency = t.Currency,
                    Comment = GetDepositOperationComment(t),
                    AmountInUsd = _rateExtractor.GetUsdEquivalent(t.Amount, t.Currency, t.Timestamp),
                    TransactionType = GetDepositOperationType(t, _deposit.ParentAccount)
                };
            return result;
        }

        private IEnumerable<DepositTransaction> ConvertExchangeTransactionToDepositTransaction(Transaction t)
        {
            var result = new List<DepositTransaction>();

            result.Add(new DepositTransaction()
            {
                    Amount = t.Amount,
                    Timestamp = t.Timestamp,
                    Currency = t.Currency,
                    Comment = GetDepositOperationComment(t),
                    AmountInUsd = _rateExtractor.GetUsdEquivalent(t.Amount, t.Currency, t.Timestamp),
                    TransactionType = DepositTransactionTypes.ОбменРасход
            });

//            result.Add(new DepositTransaction()
//            {
//                Amount = t.Amount2,
//                Timestamp = t.Timestamp,
//                Currency = (CurrencyCodes)t.Currency2,
//                Comment = GetDepositOperationComment(t),
//                AmountInUsd = _rateExtractor.GetUsdEquivalent(t.Amount2, (CurrencyCodes)t.Currency2, t.Timestamp),
//                TransactionType = DepositTransactionTypes.ОбменДоход
//            });

            return result;
        }

        private DepositTransactionTypes GetDepositOperationType(Transaction t, Account depositAccount)
        {
            return t.Operation == OperationType.Доход
                     ? DepositTransactionTypes.Проценты
                     : t.Debet == depositAccount
                         ? DepositTransactionTypes.Расход
                         : DepositTransactionTypes.Явнес;
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
