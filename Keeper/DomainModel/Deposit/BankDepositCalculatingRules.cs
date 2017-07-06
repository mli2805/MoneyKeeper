using System;

namespace Keeper.DomainModel.Deposit
{
    [Serializable]
    public class BankDepositCalculatingRules
    {
        public bool IsFactDays { get; set; } // true 28-31/365 false 30/360

        public bool EveryStartDay { get; set; } // каждое число открытия
        // и/или
        public bool EveryFirstDayOfMonth { get; set; } // каждое первое число месяца
        // и/или
        public bool EveryLastDayOfMonth { get; set; } // каждый последний день месяца
        // и эти проценты
        public bool IsCapitalized { get; set; }

        public bool IsRateFixed { get; set; }

        public bool HasAdditionalProcent { get; set; }
        public decimal AdditionalProcent { get; set; }
    }
}