using System;

namespace Keeper.DomainModel.Deposit
{
    public class DepositDailyLine
    {
        public DateTime Date { get; set; }
        public decimal Balance { get; set; }
        public decimal DepoRate { get; set; }

        public decimal DayProcents { get; set; }
        public decimal NotPaidProcents { get; set; }

        public decimal CurrencyRate { get; set; }
        public decimal DayDevaluation { get; set; }
    }
}