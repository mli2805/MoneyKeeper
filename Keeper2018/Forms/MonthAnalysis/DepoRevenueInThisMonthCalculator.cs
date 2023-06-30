using System;
using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class DepoRevenueInThisMonthCalculator
    {
        public static decimal GetRevenueInThisMonth(this AccountItemModel depo, KeeperDataModel dataModel)
        {
            var deposit = depo.Deposit;
            var depositOffer = dataModel.DepositOffers.First(o => o.Id == deposit.DepositOfferId);
            var conditions = depositOffer.CondsMap
                .OrderBy(k => k.Key)
                .LastOrDefault(e => e.Key <= deposit.StartDate).Value;
            var lastRevenueTran = dataModel.Transactions.LastOrDefault(t =>
                t.Value.MyAccount.Id == depo.Id && t.Value.Operation == OperationType.Доход).Value;
            var lastReceivedRevenueDate = lastRevenueTran?.Timestamp ?? deposit.StartDate;
            var thisMonthRevenueDate = RevenueDate(depositOffer, deposit);

            var depositBalance = new Balance();
            decimal revenue = 0;
            var depoTraffic = dataModel.Transactions.Values.OrderBy(o => o.Timestamp)
                .Where(t => t.MyAccount.Id == depo.Id ||
                            (t.MySecondAccount != null && t.MySecondAccount.Id == depo.Id)).ToList();
            var date = deposit.StartDate;
            while (date <= thisMonthRevenueDate)
            {
                foreach (var transaction in depoTraffic.Where(t => t.Timestamp.Date == date.Date))
                {
                    depo.ApplyTransaction(transaction, depositBalance);
                }

                if (date >= lastReceivedRevenueDate)
                {
                    var k = GetCoef(date, conditions.IsFactDays);
                    revenue += k * DayRevenue(depositBalance, conditions, date);
                }

                date = date.AddDays(1);
            }
            return revenue;
        }

        public static IEnumerable<Tuple<DateTime, decimal>>
            GetRevenuesInThisMonth(this AccountItemModel depo, KeeperDataModel dataModel)
        {
            var deposit = depo.Deposit;
            var depositOffer = dataModel.DepositOffers.First(o => o.Id == deposit.DepositOfferId);
            var conditions = depositOffer.CondsMap
                .OrderBy(k => k.Key)
                .LastOrDefault(e => e.Key <= deposit.StartDate).Value;
            var lastRevenueTran = dataModel.Transactions.LastOrDefault(t =>
                t.Value.MyAccount.Id == depo.Id && t.Value.Operation == OperationType.Доход).Value;
            var lastReceivedRevenueDate = lastRevenueTran?.Timestamp ?? deposit.StartDate;

            var revenueDates = GetNotPaidRevenueDates(conditions, lastReceivedRevenueDate);

            var depositBalance = new Balance();
            decimal revenue = 0;
            var depoTraffic = dataModel.Transactions.Values.OrderBy(o => o.Timestamp)
                .Where(t => t.MyAccount.Id == depo.Id ||
                            (t.MySecondAccount != null && t.MySecondAccount.Id == depo.Id)).ToList();
            var date = deposit.StartDate;
            var enumerator = revenueDates.GetEnumerator();
            if (enumerator.MoveNext()) // first time should be true
                while (date <= DateTime.Today.GetEndOfMonth())
                {
                    foreach (var transaction in depoTraffic.Where(t => t.Timestamp.Date == date.Date))
                    {
                        depo.ApplyTransaction(transaction, depositBalance);
                    }

                    if (date >= lastReceivedRevenueDate)
                    {
                        var k = GetCoef(date, conditions.IsFactDays);
                        revenue += k * DayRevenue(depositBalance, conditions, date);
                    }

                    if (date.Date == enumerator.Current.Date)
                    {
                        yield return new Tuple<DateTime, decimal>(date, revenue);
                        revenue = 0;
                        if (!enumerator.MoveNext())
                        {
                            enumerator.Dispose();
                            break;
                        }
                    }

                    date = date.AddDays(1);
                }
        }

        private static int GetCoef(DateTime date, bool isFactDays)
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

        private static void ApplyTransaction(this AccountItemModel depo, TransactionModel tran, Balance balanceBefore)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход: balanceBefore.Add(tran.Currency, tran.Amount); return;
                case OperationType.Расход: balanceBefore.Sub(tran.Currency, tran.Amount); return;
                case OperationType.Перенос:
                    if (tran.MyAccount.Id == depo.Id) balanceBefore.Sub(tran.Currency, tran.Amount);
                    if (tran.MySecondAccount.Id == depo.Id) balanceBefore.Add(tran.Currency, tran.Amount);
                    return;
                case OperationType.Обмен:
                    if (tran.MyAccount.Id == depo.Id) balanceBefore.Sub(tran.Currency, tran.Amount);
                    // ReSharper disable once PossibleInvalidOperationException
                    if (tran.MySecondAccount.Id == depo.Id)
                        balanceBefore.Add((CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                    return;
                default: return;
            }
        }

        private static decimal DayRevenue(Balance depoBalance, DepoCondsModel conditions, DateTime date)
        {
            var currentAmount = depoBalance.Currencies.FirstOrDefault(c => !c.Value.Equals(0)).Value;
            var rateLines =
                conditions.RateLines.Where(l => l.AmountFrom <= currentAmount && l.AmountTo >= currentAmount).
                    OrderBy(o => o.DateFrom).ToArray();

            int i;
            for (i = 0; i < rateLines.Length; i++)
            {
                if (rateLines[i].DateFrom > date) break;
            }
            if (i == rateLines.Length) i--;

            return currentAmount * rateLines[i].Rate / 100 / (DateTime.IsLeapYear(date.Year) ? 366 : 365);
        }

        private static DateTime RevenueDate(DepositOfferModel depositOffer, Deposit deposit)
        {
            var conditions = depositOffer.CondsMap
                .OrderBy(k => k.Key)
                .LastOrDefault(e => e.Key <= deposit.StartDate).Value;

            var thisMonth = DateTime.Today.Month;
            var thisYear = DateTime.Today.Year;

            if (conditions.EveryFirstDayOfMonth)
                return new DateTime(thisYear, thisMonth, 1);
            if (conditions.EveryLastDayOfMonth)
                return DateTime.Today.GetEndOfMonth();
            var maxDay = DateTime.DaysInMonth(thisYear, thisMonth);

            return deposit.StartDate.Day <= maxDay
                ? new DateTime(thisYear, thisMonth, deposit.StartDate.Day)
                : new DateTime(thisYear, thisMonth, maxDay);
        }

        private static IEnumerable<DateTime> GetNotPaidRevenueDates(DepoCondsModel conditions, DateTime lastReceivedRevenueDate)
        {
            while (lastReceivedRevenueDate < DateTime.Today.GetEndOfMonth())
            {
                if (conditions.EveryFirstDayOfMonth)
                    lastReceivedRevenueDate = lastReceivedRevenueDate.AddMonths(1).GetStartOfMonth();
                if (conditions.EveryLastDayOfMonth)
                    lastReceivedRevenueDate = lastReceivedRevenueDate.AddMonths(1).GetEndOfMonth();
                if (conditions.EveryStartDay)
                    lastReceivedRevenueDate = lastReceivedRevenueDate.AddMonths(1);
                if (conditions.EveryNDays)
                    lastReceivedRevenueDate = lastReceivedRevenueDate.AddDays(conditions.NDays);

                if (lastReceivedRevenueDate <= DateTime.Today.GetEndOfMonth())
                    yield return lastReceivedRevenueDate;

            }
        }
    }
}
