using System;

namespace Keeper.DomainModel
{
    public class DepositDailyLine
    {
        public DateTime Date { get; set; }
        public decimal Balance { get; set; }
        public decimal DepoRate { get; set; }
        public decimal DayProfit { get; set; }
    }
}