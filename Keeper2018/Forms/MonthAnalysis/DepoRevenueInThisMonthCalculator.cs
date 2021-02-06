using System;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class DepoRevenueInThisMonthCalculator
    {
        public static decimal GetRevenueInThisMonth(this AccountModel depo, KeeperDataModel dataModel)
        {
            var deposit = depo.Deposit;
            var depositOffer = dataModel.DepositOffers.First(o => o.Id == deposit.DepositOfferId);
            var conditionses = depositOffer.ConditionsMap.OrderBy(k => k.Key).LastOrDefault(e => e.Key <= deposit.StartDate).Value;
            var lastRevenueTran = dataModel.Transactions.LastOrDefault(t =>
                t.Value.MyAccount.Id == depo.Id && t.Value.Operation == OperationType.Доход).Value;
            var lastReceivedRevenueDate = lastRevenueTran?.Timestamp ?? deposit.StartDate;
            var thisMonthRevenueDate = RevenueDate(depositOffer, deposit);

            var depositBalance = new Balance();
            decimal revenue = 0;
            var depoTraffic = dataModel.Transactions.Values.OrderBy(o => o.Timestamp)
                .Where(t => t.MyAccount.Id == depo.Id || t.MySecondAccount.Id == depo.Id).ToList();
            var date = deposit.StartDate;
            while (date <= thisMonthRevenueDate)
            {
                foreach (var transaction in depoTraffic.Where(t => t.Timestamp.Date == date.Date))
                {
                    depo.ApplyTransaction(transaction, depositBalance);
                }

                if (date >= lastReceivedRevenueDate)
                {
                    var k = GetKoeff(date, conditionses.CalculationRules.IsFactDays);
                    revenue += k * DayRevenue(depositBalance, conditionses, date);
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

        private static void ApplyTransaction(this AccountModel depo, TransactionModel tran, Balance balanceBefore)
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
                    if (tran.MySecondAccount.Id == depo.Id) balanceBefore.Add((CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                    return;
                default: return;
            }
        }

        private static decimal DayRevenue(Balance depoBalance, DepositConditions conditionses, DateTime date)
        {
            var currentAmount = depoBalance.Currencies.FirstOrDefault(c=>!c.Value.Equals(0)).Value;
            var rateLines =
                conditionses.RateLines.Where(l => l.AmountFrom <= currentAmount && l.AmountTo >= currentAmount).
                    OrderBy(o => o.DateFrom).ToArray();

            int i;
            for (i = 0; i < rateLines.Length; i++)
            {
                if (rateLines[i].DateFrom > date) break;
            }
            if (i == rateLines.Length) i--;

            return currentAmount * rateLines[i].Rate / 100 * 1 / 365;
        }

        private static DateTime RevenueDate(DepositOfferModel depositOffer, Deposit deposit)
        {
            var conditionses = depositOffer.ConditionsMap.OrderBy(k => k.Key).LastOrDefault(e => e.Key <= deposit.StartDate).Value;
            var thisMonth = DateTime.Today.Month;
            var thisYear = DateTime.Today.Year;

            if (conditionses.CalculationRules.EveryFirstDayOfMonth)
                return new DateTime(thisYear, thisMonth, 1);
            if (conditionses.CalculationRules.EveryLastDayOfMonth)
                return DateTime.Today.GetEndOfMonthForDate();
            var maxDay = DateTime.DaysInMonth(thisYear, thisMonth);

            return deposit.StartDate.Day <= maxDay 
                ? new DateTime(thisYear, thisMonth, deposit.StartDate.Day)
                : new DateTime(thisYear, thisMonth, maxDay);
        }
    }
}
