using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Windows;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Trans;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.CommonKeeper;

namespace Keeper.Utils.OxyPlots
{
    [Export]
    public class ExpensePartingDataProvider
    {
        private readonly KeeperDb _db;
        private readonly TransactionSetConvertor _transactionSetConvertor;

        [ImportingConstructor]
        public ExpensePartingDataProvider(KeeperDb db, TransactionSetConvertor transactionSetConvertor)
        {
            _db = db;
            _transactionSetConvertor = transactionSetConvertor;
        }

        public List<ExpensePartingDataElement> Get()
        {
            var result = new List<ExpensePartingDataElement>();

            var categories = _db.Accounts.First(a => a.Name == "Все расходы").Children.ToList();
            var classifiedExpenseTransactions = GetClassifiedExpenses().ToList();

            foreach (var category in categories)
            {
                var k = category;
                var trs = from t in classifiedExpenseTransactions where t.Category == k select t;
                var r = from t in trs
                        group t by new { t.Timestamp.Month, t.Timestamp.Year } into g
                        select new ExpensePartingDataElement(k, g.Sum(a => a.AmountInUsd), new YearMonth(g.Key.Year, g.Key.Month));
                result.AddRange(r);
            }
            return result;
        }

        private IEnumerable<ClassifiedExpense> GetClassifiedExpenses()
        {
            return from t in _db.TransWithTags
                   where t.Operation == OperationType.Расход
                   join r in _db.CurrencyRates
                     on new { t.Timestamp.Date, Currency = t.Currency.GetValueOrDefault() } equals new { r.BankDay.Date, r.Currency } into g
                   from rate in g.DefaultIfEmpty()
                   select new ClassifiedExpense
                   {
                       Timestamp = t.Timestamp,
                       Amount = t.Amount,
                       Currency = t.Currency.GetValueOrDefault(),
                       Category = GetExpenseTranCategory(t),
                       AmountInUsd = rate != null ? t.Amount / (decimal)rate.Rate : t.Amount,
                       Comment = t.Comment
                   };
        }

        private Account UpToCategory(Account tag)
        {
            return tag.Parent.Name == "Все расходы" ? tag : UpToCategory(tag.Parent);
        }

        private Account GetExpenseTranCategory(TranWithTags tran)
        {
            var result = tran.Tags.FirstOrDefault(t => t.Is("Все расходы"));
            if (result != null) return UpToCategory(result);
            MessageBox.Show($"не задана категория расходов {tran.Timestamp} {tran.Amount} {tran.Currency}","");
            return null;
        }

    }
}
