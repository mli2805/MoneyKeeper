using System;
using System.Linq;

namespace Keeper2018
{
    public static class DepositOfferExt
    {

        public static decimal GetRevenueUptoDepoFinish
            (this DepositOffer depositOffer, Deposit deposit, DateTime lastReceivedRevenueDate, decimal currentAmount)
        {
            var essentials = depositOffer.Essentials.OrderBy(k => k.Key).LastOrDefault(e => e.Key <= deposit.StartDate).Value;

            var rateLines =
                essentials.RateLines.Where(l => l.AmountFrom <= currentAmount && l.AmountTo >= currentAmount).
                                                    OrderBy(o => o.DateFrom).ToArray();

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
                revenue = revenue + currentAmount * rateLines[j].Rate / 100 * days / 365;
            }

            return revenue;
        }

        public static decimal GetRevenueInThisMonth(this AccountModel depo, KeeperDb db)
        {
            var deposit = depo.Deposit;
            var depositOffer = db.Bin.DepositOffers.First(o => o.Id == deposit.DepositOfferId);
            var essentials = depositOffer.Essentials.OrderBy(k => k.Key).LastOrDefault(e => e.Key <= deposit.StartDate).Value;
            var lastRevenueTran = db.Bin.Transactions.LastOrDefault(t =>
                t.Value.MyAccount == depo.Id && t.Value.Operation == OperationType.Доход).Value;
            var lastReceivedRevenueDate = lastRevenueTran?.Timestamp ?? deposit.StartDate;
            var thisMonthRevenueDate = RevenueDate(depositOffer, deposit);

            var depositBalance = new Balance();
            decimal revenue = 0;
            var date = deposit.StartDate;
            while (date < thisMonthRevenueDate)
            {
                foreach (var transaction in db.Bin.Transactions.Values.
                    Where(t => t.Timestamp.Date == date.Date && (t.MyAccount == depo.Id || t.MySecondAccount == depo.Id)))
                {
                    depo.ApplyTransaction(transaction, depositBalance);
                }
                if (date >= lastReceivedRevenueDate)
                    revenue += DayRevenue(depositBalance, essentials, date);

                date = date.AddDays(1);
            }
            return revenue;
        }

        private static void ApplyTransaction(this AccountModel depo, Transaction tran, Balance balanceBefore)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход: balanceBefore.Add(tran.Currency, tran.Amount); return;
                case OperationType.Расход: balanceBefore.Sub(tran.Currency, tran.Amount); return;
                case OperationType.Перенос:
                    if (tran.MyAccount == depo.Id) balanceBefore.Sub(tran.Currency, tran.Amount);
                    if (tran.MySecondAccount == depo.Id) balanceBefore.Add(tran.Currency, tran.Amount);
                    return;
                case OperationType.Обмен:
                    if (tran.MyAccount == depo.Id) balanceBefore.Sub(tran.Currency, tran.Amount);
                    // ReSharper disable once PossibleInvalidOperationException
                    if (tran.MySecondAccount == depo.Id) balanceBefore.Add((CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                    return;
                default: return;
            }
        }

        private static decimal DayRevenue(Balance depoBalance, DepositEssential essentials, DateTime date)
        {
            var currentAmount = depoBalance.Currencies.First().Value;
            var rateLines =
                essentials.RateLines.Where(l => l.AmountFrom <= currentAmount && l.AmountTo >= currentAmount).
                    OrderBy(o => o.DateFrom).ToArray();

            int i;
            for (i = 0; i < rateLines.Length; i++)
            {
                if (rateLines[i].DateFrom > date) break;
            }
            if (i == rateLines.Length) i--;

            return currentAmount * rateLines[i].Rate / 100 * 1 / 365;
        }

        private static DateTime RevenueDate(DepositOffer depositOffer, Deposit deposit)
        {
            var essentials = depositOffer.Essentials.OrderBy(k => k.Key).LastOrDefault(e => e.Key <= deposit.StartDate).Value;
            var thisMonth = DateTime.Today.Month;
            var thisYear = DateTime.Today.Year;

            if (essentials.CalculationRules.EveryFirstDayOfMonth)
                return new DateTime(thisYear, thisMonth, 1);
            if (essentials.CalculationRules.EveryLastDayOfMonth)
                return DateTime.Today.GetEndOfMonthForDate();
            return new DateTime(thisYear, thisMonth, deposit.StartDate.Day);
        }
    }
}
