﻿using System;
using System.Composition;
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
        }

        private void CalculateDailyValues(Action<Deposit, DepositDailyLine> getCorrespondingDepoRate, CurrencyCodes depositCurrency)
        {
            decimal notPaidProcents = 0;
            decimal capitalizedProfit = 0;
            decimal previousBalance = 0;
            var commonFunctionProvider = IoC.Get<DepositCalculationFunctions>();
            foreach (var dailyLine in _deposit.CalculationData.DailyTable)
            {
                if (commonFunctionProvider.IsItDayToPayProcents(_deposit, dailyLine.Date))
                {
                    if (_deposit.DepositOffer.CalculatingRules.IsCapitalized)
                        capitalizedProfit += notPaidProcents;
                    notPaidProcents = 0;  // даже если нет капитализации, но есть выплата процентов, начисленные за период проценты выплачиваются на спец счет
                }

                dailyLine.Balance += capitalizedProfit;

                if (dailyLine.Date == _deposit.FinishDate) continue;

                getCorrespondingDepoRate(_deposit, dailyLine);
                commonFunctionProvider.CalculateOneDayProcents(dailyLine, _deposit.DepositOffer.CalculatingRules.IsFactDays);
                dailyLine.NotPaidProcents = notPaidProcents + dailyLine.DayProcents;
                notPaidProcents = dailyLine.NotPaidProcents;

                commonFunctionProvider.CalculateOneDayDevalvation(dailyLine, previousBalance, depositCurrency, _rateExtractor);
                previousBalance = dailyLine.Balance;

            }
        }

    }
}
