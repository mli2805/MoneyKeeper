using System;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.WorkTypes;

namespace Keeper.Utils.DiagramDataExtraction
{
    public class DataClassifiedByCategoriesElement : IComparable<DataClassifiedByCategoriesElement>
    {
        public Account Category { get; set; }
        public decimal Amount { get; set; }
        public YearMonth YearMonth { get; set; }

        public DataClassifiedByCategoriesElement(Account category, decimal amount, YearMonth yearMonth)
        {
            Category = category;
            Amount = amount;
            YearMonth = yearMonth;
        }

        /// <summary>
        /// любое сравнение инстансов в том числе поиск мин/мах запросами будет использовать это сравнение
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(DataClassifiedByCategoriesElement other)
        {
            return YearMonth.CompareTo(other.YearMonth);
        }

    }
}