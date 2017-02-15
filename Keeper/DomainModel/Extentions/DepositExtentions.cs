using System;

namespace Keeper.DomainModel.Extentions
{
    public static class DepositExtentions
    {
        public static bool IsItDayToPayProcents(this Deposit.Deposit deposit, DateTime date)
        {
            if (date == deposit.StartDate) return false;
            if (deposit.DepositOffer.CalculatingRules.EveryFirstDayOfMonth && date.Day == 1) return true;
            if (deposit.DepositOffer.CalculatingRules.EveryLastDayOfMonth && date.AddDays(1).Day == 1) return true;
            if (deposit.DepositOffer.CalculatingRules.EveryStartDay && date.Day == deposit.StartDate.Day) return true;
            if (deposit.FinishDate == date) return true;

            return false;
        }


    }
}
