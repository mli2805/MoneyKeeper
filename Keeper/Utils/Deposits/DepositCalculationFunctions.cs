using System;
using System.Composition;
using System.Linq;
using Keeper.DomainModel.Deposit;

namespace Keeper.Utils.Deposits
{
    [Export]
    public class DepositCalculationFunctions
    {
        /// <summary>
        /// стандартный фикс - ставка берется на дату открытия, от суммы не зависит
        /// АПБ Старт
        /// ВТБ Бонус
        /// </summary>
        /// <param name="deposit"></param>
        /// <param name="dailyLine"></param>
        public void GetCorrespondingDepoRateFix(Deposit deposit, DepositDailyLine dailyLine)
        {
            var line = deposit.DepositOffer.RateLines.LastOrDefault(l => l.AmountFrom <= dailyLine.Balance && l.AmountTo >= dailyLine.Balance && l.DateFrom <= deposit.StartDate);
            dailyLine.DepoRate = line == null ? 0 : line.Rate;
        }

        /// <summary>
        /// ВТБ Скарбонка - фикс первые 3 месяца, далее ставка установленная для определенной группы счетов
        /// которая отличается от ставки для вновь открываемых и ставки для других счетов открытых в другие даты
        /// 
        /// На практике - заводить для каждой группы отдельный вид вклада - Скарбонка, Скарбонка2 и т.д.
        /// Тогда таблица ставок по Скарбонка будет содержать ставки для нужного счета, при вводе ставок
        /// учитывать когда истекает 3 месяца, а не с какого числа ВТБ вводит ставку
        /// 
        /// Т.о. можно говорить что ставка не фикс, какая есть в таблице на интересующую дату
        /// такую и использовать.
        /// 
        /// ВТБ Скарбонка
        /// любая сберкарта
        /// 
        /// От суммы не зависит
        /// </summary>
        /// <param name="deposit"></param>
        /// <param name="dailyLine"></param>
        public void GetCorrespondingDepoRateNotFix(Deposit deposit, DepositDailyLine dailyLine)
        {
            var line = deposit.DepositOffer.RateLines.LastOrDefault(l => l.AmountFrom <= dailyLine.Balance && l.AmountTo >= dailyLine.Balance && l.DateFrom < dailyLine.Date);
            dailyLine.DepoRate = line == null ? 0 : line.Rate;
        }

        public bool IsItDayToPayProcents(Deposit deposit, DateTime date)
        {
            if (deposit.DepositOffer.CalculatingRules.EveryFirstDayOfMonth && date.Day == 1) return true;
            if (deposit.DepositOffer.CalculatingRules.EveryLastDayOfMonth && date.AddDays(1).Day == 1) return true;
            if (deposit.DepositOffer.CalculatingRules.EveryStartDay && date.Day == deposit.StartDate.Day) return true;
            if (deposit.FinishDate == date) return true;

            return false;
        }

        public void CalculateOneDayProcents(DepositDailyLine dailyLine, bool isFactDays)
        {
            GetBareDayProcent(dailyLine, isFactDays);
            MindBankDayPolitic(dailyLine, isFactDays);
        }

        private static void MindBankDayPolitic(DepositDailyLine dailyLine, bool isFactDays)
        {
            if (!isFactDays)
            {
                if (dailyLine.Date.Day == 31) dailyLine.DayProfit = 0;

                if (dailyLine.Date.Month == 2 && dailyLine.Date.Day == 28 && !DateTime.IsLeapYear(dailyLine.Date.Year))
                    dailyLine.DayProfit *= 3;
                if (dailyLine.Date.Month == 2 && dailyLine.Date.Day == 29 && DateTime.IsLeapYear(dailyLine.Date.Year))
                    dailyLine.DayProfit *= 2;
            }
        }

        private void GetBareDayProcent(DepositDailyLine dailyLine, bool isFactDays)
        {
            var yearDayQuantity = !isFactDays
                ? 360
                : DateTime.IsLeapYear(dailyLine.Date.Year) ? 366 : 365;

            dailyLine.DayProfit = dailyLine.Balance * dailyLine.DepoRate / 100 / yearDayQuantity;
        }

    }
}
