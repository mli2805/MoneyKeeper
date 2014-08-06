using System;
using System.Composition;
using System.Linq;
using Keeper.DomainModel.Deposit;

namespace Keeper.Utils.Deposits
{
    [Export]
    public class DepositCalculator
    {
        private readonly DepositExtractor _depositExtractor;

        private Deposit _deposit;

        [ImportingConstructor]
        public DepositCalculator(DepositExtractor depositExtractor)
        {
            _depositExtractor = depositExtractor;
        }

        public void Calculate(Deposit deposit)
        {
            _deposit = deposit;

            _depositExtractor.Extract(_deposit.ParentAccount);
            CalculateDailyProcents();
        }

        private void CalculateDailyProcents()
        {
            foreach (var day in _deposit.CalculationData.DailyTable)
            {
                day.DepoRate = GetCorrespondingDepoRate(day.Balance, day.Date);
                day.DayProfit = CalculateOneDayProcents(day.DepoRate, day.Balance);
            }
        }

        /// <summary>
        /// вот здесь реальные отличия в расчете процентов 
        /// выше по стеку идет суммирование расчитанных здесь процентов
        /// 
        /// </summary>
        /// <param name="balance"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        private decimal GetCorrespondingDepoRate(decimal balance, DateTime date)
        {
            var line = _deposit.DepositOffer.RateLines.LastOrDefault(l => l.AmountFrom <= balance && l.AmountTo >= balance && l.DateFrom < date);
            return line == null ? 0 : line.Rate;
        }

        private decimal CalculateOneDayProcents(decimal depoRate, decimal balance)
        {
            var year = _deposit.DepositOffer.CalculatingRules.IsFactDays ? 365 : 360;
            return balance * depoRate / 100 / year;
        }


    }
}
