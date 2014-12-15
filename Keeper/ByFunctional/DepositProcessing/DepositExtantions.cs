using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keeper.DomainModel.Deposit;

namespace Keeper.ByFunctional.DepositProcessing
{
    public static class DepositExtantions
    {
        public static bool IsItDayToPayProcents(this Deposit deposit, DateTime date)
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
