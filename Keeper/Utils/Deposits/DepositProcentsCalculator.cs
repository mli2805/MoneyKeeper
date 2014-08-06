using System;
using System.Collections.Generic;
using System.Composition;
using Keeper.DomainModel;
using System.Linq;
using Keeper.DomainModel.Deposit;

namespace Keeper.Utils.Deposits
{
    [Export]
    public class DepositProcentsCalculator
    {
        private readonly KeeperDb _db;

        [ImportingConstructor]
        public DepositProcentsCalculator(KeeperDb db)
        {
            _db = db;
        }

        public void FillinProcents(Account account)
        {
            CalculateDailyProcents(account.Deposit);
        }


        private void CalculateDailyProcents(Deposit deposit)
        {
            foreach (var line in deposit.CalculationData.DailyTable)
            {
                line.DepoRate = GetCorrespondingDepoRate(deposit, line.Balance, line.Date);
                line.DayProfit = CalculateOneDayProcents(deposit, line.DepoRate, line.Balance);
            }
        }


        /// <summary>
        /// вот здесь реальные отличия в расчете процентов 
        /// выше по стеку идит суммирование расчитанных здесь процентов
        /// 
        /// </summary>
        /// <param name="deposit"></param>
        /// <param name="balance"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        private decimal GetCorrespondingDepoRate(Deposit deposit, decimal balance, DateTime date)
        {
            var line = deposit.DepositOffer.RateLines.LastOrDefault(l => l.AmountFrom <= balance && l.AmountTo >= balance && l.DateFrom < date);
            return line == null ? 0 : line.Rate;
        }

        private decimal CalculateOneDayProcents(Deposit deposit, decimal depoRate, decimal balance)
        {
            //      var year = deposit.IsFactDays ? 365 : 360;
            var year = deposit.DepositOffer.CalculatingRules.IsFactDays ? 365 : 360;
            return balance * depoRate / 100 / year;
        }
    }
}
