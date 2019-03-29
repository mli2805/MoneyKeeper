using System;

namespace Keeper2018
{
    public class CategoriesDataElement : IComparable<CategoriesDataElement>
    {
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }
        public YearMonth YearMonth { get; set; }

        public CategoriesDataElement(int categoryId, decimal amount, YearMonth yearMonth)
        {
            CategoryId = categoryId;
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

    
}
