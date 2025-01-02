using System;
using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public class ForeseenIncome
    {
        public DateTime ExpectedAt;
        public decimal AmountUsd;
        public string Title;
    }

    public static class IncomeForecaster
    {
        private const int SalaryCategory = 204;
        private const int PrepaymentCategory = 1008;
        private const int IitAccountId = 443;
        private const int OptixsoftAccountId = 172;

        private const decimal IitSalary = 700;
        private const decimal OptixsoftSalary = 1100;
        private const int SalaryDay = 25;
        private const decimal IitPrepayment = 100;
        private const decimal OptixsoftPrepayment = 100;
        private const int PrepaymentDay = 5;

        public static List<ForeseenIncome> ForecastIncome2(
            this KeeperDataModel dataModel, DateTime fromDate, DateTime finishMoment)
        {
            var list = dataModel.ForeseeSalary(fromDate, finishMoment).ToList();

            var depoMainFolder = dataModel.AcMoDict[166];
            foreach (var depo in depoMainFolder.Children.Where(c => ((AccountItemModel)c).IsDeposit))
            {
                var depoForecast = dataModel.ForeseeDepoIncome2((AccountItemModel)depo);
                list.AddRange(depoForecast);
            }

            return list;
        }

        private static IEnumerable<ForeseenIncome> ForeseeSalary(
            this KeeperDataModel dataModel, DateTime firstOfMonth, DateTime finishMoment)
        {
            var receivedIncome = dataModel.Transactions.Values
                .Where(t => t.Operation == OperationType.Доход
                            && t.Timestamp >= firstOfMonth && t.Timestamp <= finishMoment).ToList();

            var salaryDate = firstOfMonth.AddDays(SalaryDay - 1);
            if (!receivedIncome.Any(t => t.Category.Id == SalaryCategory
                                             && t.Counterparty.Id == IitAccountId))
            {
                yield return new ForeseenIncome()
                {
                    ExpectedAt = salaryDate,
                    AmountUsd = IitSalary,
                    Title = $"{salaryDate:dd MMM} зарплата ИИТ {IitSalary} usd"
                };
            }
            if (!receivedIncome.Any(t => t.Category.Id == SalaryCategory
                                         && t.Counterparty.Id == OptixsoftAccountId))
            {
                yield return new ForeseenIncome()
                {
                    ExpectedAt = salaryDate,
                    AmountUsd = OptixsoftSalary,
                    Title = $"{salaryDate:dd MMM} зарплата OptixSoft {IitSalary} usd"
                };
            }

            var prepaymentDate = firstOfMonth.AddDays(PrepaymentDay - 1);
            if (!receivedIncome.Any(t => t.Category.Id == PrepaymentCategory
                                         && t.Counterparty.Id == IitAccountId))
            {
                yield return new ForeseenIncome()
                {
                    ExpectedAt = prepaymentDate,
                    AmountUsd = IitPrepayment,
                    Title = $"{prepaymentDate:dd MMM} аванс ИИТ {IitPrepayment} usd"
                };
            }
            if (!receivedIncome.Any(t => t.Category.Id == PrepaymentCategory
                                         && t.Counterparty.Id == OptixsoftAccountId))
            {
                yield return new ForeseenIncome()
                {
                    ExpectedAt = prepaymentDate,
                    AmountUsd = OptixsoftPrepayment,
                    Title = $"{prepaymentDate:dd MMM} аванс OptixSoft {OptixsoftPrepayment} usd"
                };
            }
        }

        private static IEnumerable<ForeseenIncome> ForeseeDepoIncome2(this KeeperDataModel dataModel, AccountItemModel depo)
        {
            var depoMainCurrency = dataModel.DepositOffers
                .First(o => o.Id == depo.BankAccount.DepositOfferId).MainCurrency;
            var currency = depoMainCurrency == CurrencyCode.BYR ? CurrencyCode.BYN : depoMainCurrency;

            var revenues = depo.GetRevenuesInThisMonth(dataModel);

            return revenues.Select(tuple => new ForeseenIncome
            {
                ExpectedAt = tuple.Item1,
                AmountUsd = currency == CurrencyCode.USD
                    ? tuple.Item2
                    : dataModel.AmountInUsd(DateTime.Today, depoMainCurrency, tuple.Item2),
                Title = $"{tuple.Item1:dd MMM} {depo.ShortName}  {tuple.Item2:#,0.00} {currency.ToString().ToLower()} "
            });
        }
    }
}
