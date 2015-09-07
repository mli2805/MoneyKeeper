using System;
using System.Composition;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.DomainModel.Deposit;
using Keeper.Utils.Rates;

namespace Keeper.ByFunctional.DepositProcessing
{
    [Export]
    public class DepositCalculator
    {
        private readonly DepositTrafficExtractor _depositTrafficExtractor;
        private readonly DepositTrafficEvaluator _depositTrafficEvaluator;
        private readonly RateExtractor _rateExtractor;
        private readonly DepositCalculationFunctions _depositCalculationFunctions;
        [ImportingConstructor]
        public DepositCalculator(DepositTrafficExtractor depositTrafficExtractor,
             DepositTrafficEvaluator depositTrafficEvaluator, RateExtractor rateExtractor,
             DepositCalculationFunctions depositCalculationFunctions)
        {
            _depositTrafficExtractor = depositTrafficExtractor;
            _depositTrafficEvaluator = depositTrafficEvaluator;
            _rateExtractor = rateExtractor;
            _depositCalculationFunctions = depositCalculationFunctions;
        }

        public void Calculate(Deposit deposit)
        {
            _depositTrafficEvaluator.EvaluateTraffic(_depositTrafficExtractor.ExtractTraffic(deposit.ParentAccount));
            if (deposit.CalculationData.State == DepositStates.Закрыт) return;

            if (deposit.DepositOffer.CalculatingRules.IsRateFixed)
                CalculateDailyValues(deposit, _depositCalculationFunctions.GetCorrespondingDepoRateFix);
            else
                CalculateDailyValues(deposit, _depositCalculationFunctions.GetCorrespondingDepoRateNotFix);
            deposit.CalculationData.CurrentDevaluationInUsd =
                            deposit.CalculationData.DailyTable.Where(d => d.Date <= DateTime.Today).Sum(l => l.DayDevaluation);
        }

        private void CalculateDailyValues(Deposit deposit, Action<Deposit, DepositDailyLine> getCorrespondingDepoRate)
        {
            decimal notPaidProcents = 0;
            decimal capitalizedProfit = 0;
            decimal previousBalance = 0;
            decimal previousCurrencyRate = 0;
            decimal currencyGradient = 0;
            if (deposit.DepositOffer.Currency != CurrencyCodes.USD)
            {
                currencyGradient = (decimal)
                    (_rateExtractor.GetRateThisDayOrBefore(deposit.DepositOffer.Currency, DateTime.Today) -
                     _rateExtractor.GetRateThisDayOrBefore(deposit.DepositOffer.Currency, DateTime.Today.AddDays(-30)))/ 30;
                currencyGradient = 1 + currencyGradient/(decimal)_rateExtractor.GetRateThisDayOrBefore(deposit.DepositOffer.Currency, DateTime.Today);
            }
            foreach (var dailyLine in deposit.CalculationData.DailyTable)
            {
                getCorrespondingDepoRate(deposit, dailyLine);
                dailyLine.DayProcents = _depositCalculationFunctions.CalculateOneDayProcents(previousBalance, dailyLine.Date, 
                                                           dailyLine.DepoRate, deposit.DepositOffer.CalculatingRules.IsFactDays);
                dailyLine.NotPaidProcents = notPaidProcents + dailyLine.DayProcents;
                notPaidProcents = dailyLine.NotPaidProcents;

                if (deposit.IsItDayToPayProcents(dailyLine.Date))
                {
                    if (deposit.DepositOffer.CalculatingRules.IsCapitalized)
                        capitalizedProfit += notPaidProcents;
                    notPaidProcents = 0;  // даже если нет капитализации, но есть выплата процентов, начисленные за период проценты выплачиваются на спец счет
                }

                if (deposit.DepositOffer.Currency != CurrencyCodes.USD)
                {
                    // на этапе DepositTrafficEvaluator курсы в таблицу загнаны left outer join - могут быть нули
                    if (dailyLine.CurrencyRate == 0)
                        dailyLine.CurrencyRate = dailyLine.Date > DateTime.Today ? previousCurrencyRate * currencyGradient : previousCurrencyRate;
                    if (previousBalance != 0)
                        dailyLine.DayDevaluation = previousBalance / dailyLine.CurrencyRate - previousBalance / previousCurrencyRate;
                    previousCurrencyRate = dailyLine.CurrencyRate;
                }
                previousBalance = dailyLine.Balance;
            }
        }

    }
}
