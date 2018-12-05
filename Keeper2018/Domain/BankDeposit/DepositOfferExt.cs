using System;
using System.Linq;

namespace Keeper2018
{
    public static class DepositOfferExt
    {
        
        public static decimal GetRevenue(this DepositOffer depositOffer, Deposit deposit, DateTime lastReceivedRevenueDate, decimal amount)
        {
            var essentials = depositOffer.Essentials.OrderBy(k => k.Key).LastOrDefault(e => e.Key <= deposit.StartDate).Value;

            var rateLines = essentials.RateLines.Where(l => l.AmountFrom <= amount && l.AmountTo >= amount).OrderBy(o=>o.DateFrom).ToArray();

            decimal revenue = 0;
            int i;
            for (i = 0; i < rateLines.Length; i++)
            {
                if (rateLines[i].DateFrom > lastReceivedRevenueDate) break;
            }

            if (i == rateLines.Length) i--;

            var startPeriod = lastReceivedRevenueDate;
            for (int j = i; j < rateLines.Length; j++)
            {
                var endOfPeriod = deposit.FinishDate;
                if (rateLines.Length > j + 1 && rateLines[j + 1].DateFrom < endOfPeriod)
                    endOfPeriod = rateLines[j + 1].DateFrom;

                var days = (endOfPeriod - startPeriod).Days;
                revenue = revenue + amount * rateLines[j].Rate / 100 * days / 365;
            }

            return revenue;
        }
    }
}
