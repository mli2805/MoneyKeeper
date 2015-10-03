using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.Utils.Common;
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

        private IEnumerable<Account> GetExpenseKategories()
        {
            var expense = _db.Accounts.First(a => a.Name == "Все расходы");
            return expense.Children.ToList();
        }

        public List<ExpensePartingDataElement> Get()
        {
            var result = new List<ExpensePartingDataElement>();
            var kategories = GetExpenseKategories();
            var convertedExpenseTransactions =
                _transactionSetConvertor.ConvertTransactionsQuery(_db.Transactions.Where(t => t.Operation == OperationType.Расход && !t.IsExchange())).ToList();
            foreach (var kategory in kategories)
            {
                var k = kategory;
                var trs = from t in convertedExpenseTransactions where t.Article.Is(k) select t;
                var r = from t in trs
                        group t by new { t.Timestamp.Month, t.Timestamp.Year } into g
                        select new ExpensePartingDataElement(k, g.Sum(a => a.AmountInUsd), new YearMonth(g.Key.Year, g.Key.Month));
                result.AddRange(r);
            }
            return result;
        }
    }
}
