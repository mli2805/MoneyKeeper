using System;
using System.Composition;
using Caliburn.Micro;
using Keeper.DomainModel.Deposit;
using Keeper.Utils.Deposits.BankDepositOffers;

namespace Keeper.Utils.Deposits
{
    [Export]
    public class DepositCalculator
    {
        private readonly DepositExtractor _depositExtractor;
        private readonly DepositCalculationFunctions _depositCalculationFunctions;
        private Deposit _deposit;
        [ImportingConstructor]
        public DepositCalculator(DepositExtractor depositExtractor, DepositCalculationFunctions depositCalculationFunctions)
        {
            _depositExtractor = depositExtractor;
            _depositCalculationFunctions = depositCalculationFunctions;
        }

        public void Calculate(Deposit deposit)
        {
            _depositExtractor.Extract(deposit.ParentAccount);
            _deposit = deposit;

            if ((deposit.DepositOffer.BankAccount.Name == "АПБ" && deposit.DepositOffer.DepositTitle == "Старт!") ||
                 (deposit.DepositOffer.BankAccount.Name == "ВТБ-Беларусь" && deposit.DepositOffer.DepositTitle == "Бонус"))
                                           CalculateDailyProcents(_depositCalculationFunctions.GetCorrespondingDepoRateFix);
            if (deposit.DepositOffer.BankAccount.Name == "ВТБ-Беларусь" && deposit.DepositOffer.DepositTitle == "Скарбонка")
                                           CalculateDailyProcents(_depositCalculationFunctions.GetCorrespondingDepoRateFix3Month);
        }

        private void CalculateDailyProcents(Action<Deposit, DepositDailyLine> getCorrespondingDepoRate)
        {
            decimal notPaidProfit = 0;
            decimal capitalizedProfit = 0;
            foreach (var dailyLine in _deposit.CalculationData.DailyTable)
            {
                var commonFunctionProvider = IoC.Get<DepositCalculationFunctions>();

                if (commonFunctionProvider.IsProcentDay(_deposit, dailyLine.Date))
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
