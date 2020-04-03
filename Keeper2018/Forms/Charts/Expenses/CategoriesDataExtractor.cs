using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public class CategoriesDataExtractor
    {
        private readonly KeeperDb _db;
        private List<int> _expenseGroupsIds;

        public CategoriesDataExtractor(KeeperDb db)
        {
            _db = db;

        }

        public List<CategoriesDataElement> GetExpenseGrouppedByCategoryAndMonth()
        {
            _expenseGroupsIds = _db.AcMoDict[189].Children.Select(c=>c.Id).ToList();
            var result = new List<CategoriesDataElement>();

            var classifiedTrans = GetClassifiedTrans().ToList();

            foreach (var category in _expenseGroupsIds)
            {
                var k = category;
                var trs = from t in classifiedTrans where t.CategoryId == k select t;
                var r = from t in trs
                    group t by new { t.Timestamp.Month, t.Timestamp.Year } into g
                    select new CategoriesDataElement(k, g.Sum(a => a.AmountInUsd), new YearMonth(g.Key.Year, g.Key.Month));
                result.AddRange(r);
            }
            return result;
        }


        private IEnumerable<ClassifiedTran> GetClassifiedTrans()
        {
            foreach (var tr in _db.Bin.Transactions.Values.Where(t=>t.Operation == OperationType.Расход))
            {
                yield return new ClassifiedTran()
                {
                    Timestamp = tr.Timestamp,
                    CategoryId = tr.GetTransactionExpenseCategory(_db, _expenseGroupsIds),
                    AmountInUsd = tr.Currency == CurrencyCode.USD 
                        ? tr.Amount 
                        : _db.AmountInUsd(tr.Timestamp, tr.Currency, tr.Amount),
                };
            }
        }

    }
}
