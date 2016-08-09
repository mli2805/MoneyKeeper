using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Windows;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Extentions;
using Keeper.DomainModel.Trans;
using Keeper.DomainModel.WorkTypes;

namespace Keeper.Utils.DiagramDataExtraction
{
    [Export]
    public class CategoriesDataExtractor
    {
        private readonly KeeperDb _db;

        [ImportingConstructor]
        public CategoriesDataExtractor(KeeperDb db)
        {
            _db = db;
        }

        public List<CategoriesDataElement> GetGrouppedByMonth(bool isIncome)
        {
            return GetGrouppedByMonth(isIncome, new DateTime(9999, 1, 1));
        }

        /// <summary>
        /// В зависимости от флага возвращает ежемесячные суммы операций дохода либо расхода в разрезе категорий
        /// </summary>
        /// <param name="isIncome"></param>
        /// <param name="date">если год = 9999 - за весь период, иначе за один месяц</param>
        /// <returns></returns>
        public List<CategoriesDataElement> GetGrouppedByMonth(bool isIncome, DateTime date)
        {
            var result = new List<CategoriesDataElement>();

            var rootName = isIncome ? "Все доходы" : "Все расходы";
            var categories = _db.Accounts.First(a => a.Name == rootName).Children.ToList();
            var classifiedTrans = GetClassifiedByCategories(isIncome, date).ToList();

            foreach (var category in categories)
            {
                var k = category;
                var trs = from t in classifiedTrans where t.Category == k select t;
                var r = from t in trs
                        group t by new { t.Timestamp.Month, t.Timestamp.Year } into g
                        select new CategoriesDataElement(k, g.Sum(a => a.AmountInUsd), new YearMonth(g.Key.Year, g.Key.Month));
                result.AddRange(r);
            }
            return result;
        }

        /// <summary>
        /// В зависимости от флага возвращает все операции дохода или расхода 
        /// с суммой в долларах на дату операции для дальнейшей обработки
        /// </summary>
        /// <param name="isIncome">true - income; false - expense</param>
        /// <param name="date">если год = 9999 - за весь период, иначе за один месяц </param>
        /// <returns></returns>
        private IEnumerable<ClassifiedTran> GetClassifiedByCategories(bool isIncome, DateTime date)
        {
            return from t in _db.TransWithTags
                   where (t.Operation == (isIncome ? OperationType.Доход : OperationType.Расход )) && (date.Year == 9999 || date.IsMonthTheSame(t.Timestamp))
                   join r in _db.CurrencyRates
                     on new { t.Timestamp.Date, Currency = t.Currency.GetValueOrDefault() } equals new { r.BankDay.Date, r.Currency } into g
                   from rate in g.DefaultIfEmpty()
                   select new ClassifiedTran
                   {
                       Timestamp = t.Timestamp,
                       Category = t.GetTranCategory(isIncome),
                       AmountInUsd = rate != null ? t.Amount / (decimal)rate.Rate : t.Amount,
                   };
        }

    }
}
