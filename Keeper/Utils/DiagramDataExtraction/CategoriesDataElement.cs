using System;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.WorkTypes;

namespace Keeper.Utils.DiagramDataExtraction
{
    public class CategoriesDataElement : IComparable<CategoriesDataElement>
    {
        public Account Category { get; set; }
        public decimal Amount { get; set; }
        public YearMonth YearMonth { get; set; }

        public CategoriesDataElement(Account category, decimal amount, YearMonth yearMonth)
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
        public int CompareTo(CategoriesDataElement other)
        {
            return YearMonth.CompareTo(other.YearMonth);
        }

    }

    public class ClassifiedTran
    {
        public DateTime Timestamp { get; set; }
        public decimal AmountInUsd { get; set; }
        public Account Category { get; set; }

    }

}