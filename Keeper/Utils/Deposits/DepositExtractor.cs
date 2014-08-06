using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.DomainModel.Deposit;
using Keeper.Utils.Rates;

namespace Keeper.Utils.Deposits
{
    [Export]
    public class DepositExtractor : PropertyChangedBase
    {
        private readonly KeeperDb _db;
        private readonly RateExtractor _rateExtractor;

        private Deposit _deposit;

        [ImportingConstructor]
        public DepositExtractor(KeeperDb db, RateExtractor rateExtractor)
        {
            _db = db;
            _rateExtractor = rateExtractor;
        }

        public Deposit Extract(Account account)
        {
            _deposit = account.Deposit;

            _deposit.CalculationData = new DepositCalculationData();
            ExtractTraffic();
            EvaluateTraffic();
            FillinDailyBalances();
            DefineCurrentState();
            return _deposit;
        }

        private void ExtractTraffic()
        {
            _deposit.CalculationData.Traffic = (from t in _db.Transactions
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

        private void EvaluateTraffic()
        {
            _deposit.CalculationData.TotalMyIns = _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.Явнес).Sum(t => t.Amount);
            _deposit.CalculationData.TotalMyOuts = _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.Расход).Sum(t => t.Amount);
            _deposit.CalculationData.TotalPercent = _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.Проценты).Sum(t => t.Amount);

            _deposit.CalculationData.CurrentProfit = _rateExtractor.GetUsdEquivalent(_deposit.CalculationData.CurrentBalance, _deposit.DepositOffer.Currency, DateTime.Today)
                                    - _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.Явнес).Sum(t => t.AmountInUsd)
                                    + _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.Расход).Sum(t => t.AmountInUsd);
        }

        private void FillinDailyBalances()
        {
            var period = new Period(_deposit.StartDate, _deposit.FinishDate);
            _deposit.CalculationData.DailyTable = new List<DepositDailyLine>();
            decimal balance = 0;

            foreach (DateTime day in period)
            {
                var date = day;
                _deposit.CalculationData.DailyTable.Add(new DepositDailyLine { Date = day, Balance = balance });
                balance += _deposit.CalculationData.Traffic.Where(t => t.Timestamp.Date == date.Date).Sum(t => t.Amount*t.Destination());
            }
        }

        private void DefineCurrentState()
        {
            if (_deposit.CalculationData.CurrentBalance == 0)
                _deposit.CalculationData.State = DepositStates.Закрыт;
            else
                _deposit.CalculationData.State = _deposit.FinishDate < DateTime.Today ? DepositStates.Просрочен : DepositStates.Открыт;
        }

    }
}
