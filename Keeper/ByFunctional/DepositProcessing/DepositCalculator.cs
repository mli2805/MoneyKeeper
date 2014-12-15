﻿using System;
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
        private Deposit _deposit;
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
            _deposit = _depositTrafficEvaluator.EvaluateTraffic(_depositTrafficExtractor.ExtractTraffic(deposit.ParentAccount));

            if (deposit.DepositOffer.CalculatingRules.IsRateFixed)
                CalculateDailyValues(_depositCalculationFunctions.GetCorrespondingDepoRateFix, deposit.DepositOffer.Currency);
            else
                CalculateDailyValues(_depositCalculationFunctions.GetCorrespondingDepoRateNotFix, deposit.DepositOffer.Currency);
            deposit.CalculationData.CurrentDevaluationInUsd =
                            deposit.CalculationData.DailyTable.Where(d => d.Date <= DateTime.Today).Sum(l => l.DayDevaluation);
        }

        private void CalculateDailyValues(Action<Deposit, DepositDailyLine> getCorrespondingDepoRate, CurrencyCodes depositCurrency)
        {
            decimal notPaidProcents = 0;
            decimal capitalizedProfit = 0;
            decimal previousBalance = 0;
            decimal previousCurrencyRate = 0;
            foreach (var dailyLine in _deposit.CalculationData.DailyTable)
            {
                getCorrespondingDepoRate(_deposit, dailyLine);
                dailyLine.DayProcents = _depositCalculationFunctions.CalculateOneDayProcents(previousBalance, dailyLine.Date, 
                                                           dailyLine.DepoRate, _deposit.DepositOffer.CalculatingRules.IsFactDays);
                dailyLine.NotPaidProcents = notPaidProcents + dailyLine.DayProcents;
                notPaidProcents = dailyLine.NotPaidProcents;

                if (_deposit.IsItDayToPayProcents(dailyLine.Date))
                {
                    if (_deposit.DepositOffer.CalculatingRules.IsCapitalized)
                        capitalizedProfit += notPaidProcents;
                    notPaidProcents = 0;  // даже если нет капитализации, но есть выплата процентов, начисленные за период проценты выплачиваются на спец счет
                }

//                dailyLine.Balance += capitalizedProfit;

                if (depositCurrency != CurrencyCodes.USD)
                {
                    var currencyGradient = (decimal)(_rateExtractor.GetRateThisDayOrBefore(depositCurrency, DateTime.Today) -
                                            _rateExtractor.GetRateThisDayOrBefore(depositCurrency, DateTime.Today.AddDays(-30)))/30;
                    // на этапе DepositTrafficEvaluator курсы в таблицу загнаны left outer join - могут быть нули
                    if (dailyLine.CurrencyRate == 0)
                        dailyLine.CurrencyRate = dailyLine.Date > DateTime.Today ? previousCurrencyRate + currencyGradient : previousCurrencyRate;
                    if (previousBalance != 0) _depositCalculationFunctions.CalculateOneDayDevalvation(dailyLine, previousBalance, previousCurrencyRate);
                    previousCurrencyRate = dailyLine.CurrencyRate;
                }
                previousBalance = dailyLine.Balance;
            }
        }

    }
}
