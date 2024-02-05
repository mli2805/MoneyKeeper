using System;
using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class IncomeForecaster
    {
        public static Tuple<List<string>, decimal> ForecastIncome(this KeeperDataModel dataModel, DateTime fromDate, DateTime finishMoment)
        {
            var list = new List<string>();
            decimal total = 0;

            var realIncomes = dataModel.Transactions.Values.Where(t => t.Operation == OperationType.Доход
                                                                        && t.Timestamp >= fromDate && t.Timestamp <= finishMoment).ToList();
            var salaryAccountId = 204;
            var prepaymentAccountId = 1008;
            var iitAccountId = 443;
            var optixsoftAccountId = 172;
            if (!realIncomes.Any(t => t.Tags.Select(tt => tt.Id).Contains(salaryAccountId)
                                      && t.Tags.Select(tt => tt.Id).Contains(iitAccountId)))
            {
                var salaryInUsdValue = 700;
                list.Add($"зарплата ИИТ {salaryInUsdValue} usd");
                total += salaryInUsdValue;
            }
            if (!realIncomes.Any(t => t.Tags.Select(tt => tt.Id).Contains(salaryAccountId)
                                      && t.Tags.Select(tt => tt.Id).Contains(optixsoftAccountId)))
            {
                var salaryInUsdValue = 1100;
                list.Add($"зарплата OptixSoft {salaryInUsdValue} usd");
                total += salaryInUsdValue;
            }
            if (!realIncomes.Any(t => t.Tags.Select(tt => tt.Id).Contains(prepaymentAccountId)
                                                 && t.Tags.Select(tt => tt.Id).Contains(iitAccountId)))
            {
                var salaryInUsdValue = 100;
                list.Add($"аванс ИИТ {salaryInUsdValue} usd");
                total += salaryInUsdValue;
            }
            if (!realIncomes.Any(t => t.Tags.Select(tt => tt.Id).Contains(prepaymentAccountId)
                                      && t.Tags.Select(tt => tt.Id).Contains(optixsoftAccountId)))
            {
                var salaryInUsdValue = 100;
                list.Add($"аванс OptixSoft {salaryInUsdValue} usd");
                total += salaryInUsdValue;
            }

            var depoMainFolder = dataModel.AcMoDict[166];
            foreach (var depo in depoMainFolder.Children.Where(c => ((AccountItemModel)c).IsDeposit))
            {
                var depoForecast = dataModel.ForeseeDepoIncome((AccountItemModel)depo);
                list.AddRange(depoForecast.Item1);
                total += depoForecast.Item2;
            }

            return new Tuple<List<string>, decimal>(list, total);
        }

        private static Tuple<List<string>, decimal> ForeseeDepoIncome(this KeeperDataModel dataModel, AccountItemModel depo)
        {
            var list = new List<string>();
            decimal total = 0;

            var depoMainCurrency = dataModel.DepositOffers
                .First(o => o.Id == depo.BankAccount.DepositOfferId).MainCurrency;
            var currency = depoMainCurrency == CurrencyCode.BYR ? CurrencyCode.BYN : depoMainCurrency;

            var revenues = depo.GetRevenuesInThisMonth(dataModel);
            foreach (var tuple in revenues)
            {
                list.Add($"{depo.ShortName}  {tuple.Item2:#,0.00} {currency.ToString().ToLower()} {tuple.Item1:dd MMM}");
                total += currency == CurrencyCode.USD
                    ? tuple.Item2
                    : dataModel.AmountInUsd(DateTime.Today, depoMainCurrency, tuple.Item2);
            }

            return new Tuple<List<string>, decimal>(list, total);
        }
    }
}
