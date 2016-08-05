using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Windows;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Trans;
using Keeper.DomainModel.WorkTypes;

namespace Keeper.Utils.DiagramDataExtraction
{
    [Export]
    public class DataClassifiedByCategoriesProvider
    {
        private readonly KeeperDb _db;

        [ImportingConstructor]
        public DataClassifiedByCategoriesProvider(KeeperDb db)
        {
            _db = db;
        }

        /// <summary>
        /// В зависимости от флага возвращает ежемесячные суммы операций дохода либо расхода в разрезе категорий
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public List<DataClassifiedByCategoriesElement> GetGrouppedByMonth(bool flag)
        {
            var result = new List<DataClassifiedByCategoriesElement>();

            var rootName = flag ? "Все доходы" : "Все расходы";
            var categories = _db.Accounts.First(a => a.Name == rootName).Children.ToList();
            var classifiedTrans = GetClassifiedByCategories(flag).ToList();

            foreach (var category in categories)
            {
                var k = category;
                var trs = from t in classifiedTrans where t.Category == k select t;
                var r = from t in trs
                        group t by new { t.Timestamp.Month, t.Timestamp.Year } into g
                        select new DataClassifiedByCategoriesElement(k, g.Sum(a => a.AmountInUsd), new YearMonth(g.Key.Year, g.Key.Month));
                result.AddRange(r);
            }
            return result;
        }

        /// <summary>
        /// В зависимости от флага возвращает все операции дохода или расхода 
        /// с суммой в долларах на дату операции для дальнейшей обработки
        /// </summary>
        /// <param name="flag">true - income; false - expense</param>
        /// <returns></returns>
        private IEnumerable<ClassifiedTran> GetClassifiedByCategories(bool flag)
        {
            return from t in _db.TransWithTags
                   where t.Operation == (flag ? OperationType.Доход : OperationType.Расход)
                   join r in _db.CurrencyRates
                     on new { t.Timestamp.Date, Currency = t.Currency.GetValueOrDefault() } equals new { r.BankDay.Date, r.Currency } into g
                   from rate in g.DefaultIfEmpty()
                   select new ClassifiedTran
                   {
                       Timestamp = t.Timestamp,
                       Category = GetTranCategory(t, flag),
                       AmountInUsd = rate != null ? t.Amount / (decimal)rate.Rate : t.Amount,
                   };
        }

        private Account UpToCategory(Account tag, string root)
        {
            return tag.Parent.Name == root ? tag : UpToCategory(tag.Parent, root);
        }

        private Account GetTranCategory(TranWithTags tran, bool flag)
        {
            var rootName = flag ? "Все доходы" : "Все расходы";
            var result = tran.Tags.FirstOrDefault(t => t.Is(rootName));
            if (result != null) return UpToCategory(result, rootName);
            MessageBox.Show($"не задана категория {tran.Timestamp} {tran.Amount} {tran.Currency}", "");
            return null;

        }
    }
}
