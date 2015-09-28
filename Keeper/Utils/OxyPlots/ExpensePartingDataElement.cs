using System;
using Keeper.DomainModel;

namespace Keeper.Utils.OxyPlots
{
    public class ExpensePartingDataElement : IComparable<ExpensePartingDataElement>
    {
        public Account Kategory { get; set; }
        public decimal Amount { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        public ExpensePartingDataElement(Account kategory, decimal amount, int month, int year)
        {
            Kategory = kategory;
            Month = month;
            Amount = amount;
            Year = year;
        }

        private DateTime GetDateTime() { return new DateTime(Year,Month,1);}

        /// <summary>
        /// любое сравнение инстансов в том числе поиск мин/мах запросами будет использовать это сравнение
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(ExpensePartingDataElement other)
        {
            return this.GetDateTime().CompareTo(other.GetDateTime());
        }

    }
}