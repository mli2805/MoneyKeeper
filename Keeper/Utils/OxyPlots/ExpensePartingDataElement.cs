using System;
using Keeper.DomainModel;
using Keeper.Utils.Common;

namespace Keeper.Utils.OxyPlots
{
    public class ExpensePartingDataElement : IComparable<ExpensePartingDataElement>
    {
        public Account Kategory { get; set; }
        public decimal Amount { get; set; }
        public YearMonth YearMonth { get; set; }

        public ExpensePartingDataElement(Account kategory, decimal amount, YearMonth yearMonth)
        {
            Kategory = kategory;
            Amount = amount;
            YearMonth = yearMonth;
        }

        /// <summary>
        /// любое сравнение инстансов в том числе поиск мин/мах запросами будет использовать это сравнение
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(ExpensePartingDataElement other)
        {
            return YearMonth.CompareTo(other.YearMonth);
        }

    }
}