using System;
using System.Linq;
using KeeperDomain;

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
            var depoTraffic = db.Bin.Transactions.Values.OrderBy(o => o.Timestamp)
                .Where(t => t.MyAccount == depo.Id || t.MySecondAccount == depo.Id).ToList();
            var date = deposit.StartDate;
            while (date <= thisMonthRevenueDate)
            {
                foreach (var transaction in depoTraffic.Where(t => t.Timestamp.Date == date.Date))
                {
                    depo.ApplyTransaction(transaction, depositBalance);
                }

                if (date >= lastReceivedRevenueDate)
                {
                    var k = GetKoeff(date, essentials.CalculationRules.IsFactDays);
                    revenue += k * DayRevenue(depositBalance, essentials, date);
                }

                date = date.AddDays(1);
            }
            return revenue;
        }

        private static int GetKoeff(DateTime date, bool isFactDays)
        {
            if (isFactDays) return 1;
            if (date.Day == 31) return 0;
            if (DateTime.IsLeapYear(date.Year))
            {
                if (date.Month == 2 && date.Day == 29)
                    return 2;
            }
            else
            {
                if (date.Month == 2 && date.Day == 28)
                    return 3;
            }
            return 1;
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
            var currentAmount = depoBalance.Currencies.FirstOrDefault(c=>!c.Value.Equals(0)).Value;
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
            var maxDay = DateTime.DaysInMonth(thisYear, thisMonth);

            return deposit.StartDate.Day <= maxDay 
                ? new DateTime(thisYear, thisMonth, deposit.StartDate.Day)
                : new DateTime(thisYear, thisMonth, maxDay);
        }
    }
}
