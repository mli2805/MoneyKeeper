using System;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.DomainModel.Deposit;
using Keeper.Utils.Rates;

namespace Keeper.ByFunctional.DepositProcessing
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
        /// Если ставка в день D была X % , а в день D+1 стала Y %, 
        /// то запрос ставки для дня D+1 должен вернуть X %,
        /// т.к. проценты за ночь с D на D+1 начисляются по ставке X %
        /// 
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
            var line = deposit.DepositOffer.RateLines.LastOrDefault(l => l.AmountFrom <= dailyLine.Balance && l.AmountTo >= dailyLine.Balance &&
                //  знак равно, т.к. эта ночь еще под старую ставку
                                                                                                                     l.DateFrom <= dailyLine.Date);
            dailyLine.DepoRate = line == null ? 0 : line.Rate;
        }

        public void CalculateOneDayDevalvation(DepositDailyLine dailyLine, decimal previousBalance, decimal previousCurrencyRate)
        {
            dailyLine.DayDevaluation = previousBalance / dailyLine.CurrencyRate - previousBalance / previousCurrencyRate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="balance"></param>
        /// <param name="date"></param>
        /// <param name="rate"></param>
        /// <param name="isFactDays"></param>
        /// <returns></returns>
        public decimal CalculateOneDayProcents(decimal balance, DateTime date, decimal rate, bool isFactDays)
        {
            var dayProcents = balance * rate / 100 / GetBankDayInYear(date.Year, isFactDays);
            return isFactDays ?
                dayProcents :
                MindBankDayPolitic(dayProcents, date);
        }

        private static decimal MindBankDayPolitic(decimal dayProcents, DateTime date)
        {
            if (date.Day == 31) return 0;

            if (date.Month == 2 && date.Day == 28 && !DateTime.IsLeapYear(date.Year))
                return dayProcents * 3;
            if (date.Month == 2 && date.Day == 29 && DateTime.IsLeapYear(date.Year))
                return dayProcents * 2;

            return dayProcents;
        }

        private static int GetBankDayInYear(int year, bool isFactDays)
        {
            return !isFactDays
                ? 360
                : DateTime.IsLeapYear(year) ? 366 : 365;
        }

    }
}
