using System;
using System.Composition;
using Caliburn.Micro;
using Keeper.DomainModel.Deposit;

namespace Keeper.ByFunctional.DepositProcessing
{
    [Export]
    public class DepositCalculator
    {
        private readonly DepositTrafficExtractor _depositTrafficExtractor;
        private readonly DepositTrafficEvaluator _depositTrafficEvaluator;
        private readonly DepositCalculationFunctions _depositCalculationFunctions;
        private Deposit _deposit;
        [ImportingConstructor]
        public DepositCalculator(DepositTrafficExtractor depositTrafficExtractor, 
             DepositTrafficEvaluator depositTrafficEvaluator, DepositCalculationFunctions depositCalculationFunctions)
        {
            _depositTrafficExtractor = depositTrafficExtractor;
            _depositTrafficEvaluator = depositTrafficEvaluator;
            _depositCalculationFunctions = depositCalculationFunctions;
        }

        public void Calculate(Deposit deposit)
        {
            _deposit = _depositTrafficEvaluator.EvaluateTraffic(_depositTrafficExtractor.ExtractTraffic(deposit.ParentAccount));

            if (deposit.DepositOffer.CalculatingRules.IsRateFixed)
                                           CalculateDailyProcents(_depositCalculationFunctions.GetCorrespondingDepoRateFix);
            else
                                           CalculateDailyProcents(_depositCalculationFunctions.GetCorrespondingDepoRateNotFix);
        }

        private void CalculateDailyProcents(Action<Deposit, DepositDailyLine> getCorrespondingDepoRate)
        {
            decimal notPaidProfit = 0;
            decimal capitalizedProfit = 0;
            foreach (var dailyLine in _deposit.CalculationData.DailyTable)
            {
                var commonFunctionProvider = IoC.Get<DepositCalculationFunctions>();

                if (commonFunctionProvider.IsItDayToPayProcents(_deposit, dailyLine.Date))
                {
                    if (_deposit.DepositOffer.CalculatingRules.IsCapitalized)
                        capitalizedProfit += notPaidProfit;
                    notPaidProfit = 0;  // даже если нет капитализации, но есть выплата процентов, начисленные за период проценты выплачиваются на спец счет
                }

                dailyLine.Balance += capitalizedProfit;

                if (dailyLine.Date == _deposit.FinishDate) continue;

                getCorrespondingDepoRate(_deposit, dailyLine);
                commonFunctionProvider.CalculateOneDayProcents(dailyLine, _deposit.DepositOffer.CalculatingRules.IsFactDays);
                dailyLine.NotPaidProfit = notPaidProfit + dailyLine.DayProfit;
                notPaidProfit = dailyLine.NotPaidProfit;
            }
        }

    }
}
