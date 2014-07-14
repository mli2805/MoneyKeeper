using System;
using System.Composition;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils.Rates;

namespace Keeper.Utils.Deposits
{
    [Export]
    public class DepositExtractor : PropertyChangedBase
    {
        private readonly KeeperDb _db;
        private readonly RateExtractor _rateExtractor;
        private readonly DepositParser _depositParser;

        [ImportingConstructor]
        public DepositExtractor(KeeperDb db, RateExtractor rateExtractor, DepositParser depositParser)
        {
            _db = db;
            _rateExtractor = rateExtractor;
            _depositParser = depositParser;
        }

        public Deposit Extract(Account account)
        {
            if (account.Deposit == null)
            {
                account.Deposit = new Deposit { ParentAccount = account };
                _depositParser.ExtractInfoFromName(account);
            }

            account.Deposit.CalculatedTotals = new DepositCalculatedTotals();

            ExtractTraffic(account);

            if (account.Deposit.CalculatedTotals.Traffic.Count == 0) MessageBox.Show("Нет движения по счету!");

            account.Deposit.Currency = account.Deposit.CalculatedTotals.Traffic.First().Currency;

            EvaluateTraffic(account);
            DefineCurrentState(account);
            //            if (account.Deposit.CalculatedTotals.State != DepositStates.Закрыт) MakeForecast(account);
            return account.Deposit;
        }

        private void ExtractTraffic(Account account)
        {
            account.Deposit.CalculatedTotals.Traffic = (from t in _db.Transactions
                                                   where t.Debet == account || t.Credit == account
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
                                                       TransactionType = GetDepositOperationType(t, account)
                                                   }).ToList();
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

        private void EvaluateTraffic(Account account)
        {
            account.Deposit.CalculatedTotals.TotalMyIns = account.Deposit.CalculatedTotals.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.Явнес).Sum(t => t.Amount);
            account.Deposit.CalculatedTotals.TotalMyOuts = account.Deposit.CalculatedTotals.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.Расход).Sum(t => t.Amount);
            account.Deposit.CalculatedTotals.TotalPercent = account.Deposit.CalculatedTotals.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.Проценты).Sum(t => t.Amount);

            account.Deposit.CalculatedTotals.CurrentProfit = _rateExtractor.GetUsdEquivalent(account.Deposit.CalculatedTotals.CurrentBalance, account.Deposit.Currency, DateTime.Today)
                                    - account.Deposit.CalculatedTotals.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.Явнес).Sum(t => t.AmountInUsd)
                                    + account.Deposit.CalculatedTotals.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.Расход).Sum(t => t.AmountInUsd);
        }

        private void DefineCurrentState(Account account)
        {
            if (account.Deposit.CalculatedTotals.CurrentBalance == 0)
                account.Deposit.CalculatedTotals.State = DepositStates.Закрыт;
            else
                account.Deposit.CalculatedTotals.State = account.Deposit.FinishDate < DateTime.Today ? DepositStates.Просрочен : DepositStates.Открыт;
        }


    }
}
