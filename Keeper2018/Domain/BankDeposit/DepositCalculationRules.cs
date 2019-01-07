using System;

namespace Keeper2018
{
    [Serializable]
    public class DepositCalculationRules
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
        public double AdditionalProcent { get; set; }

        public DepositCalculationRules ShallowCopy()
        {
            return (DepositCalculationRules)MemberwiseClone();
        }

        public string Dump()
        {
            var result = "";

            result += IsFactDays ? "1" : "0";
            result += EveryStartDay ? "1" : "0";
            result += EveryFirstDayOfMonth ? "1" : "0";
            result += EveryLastDayOfMonth ? "1" : "0";
            result += IsCapitalized ? "1" : "0";
            result += IsRateFixed ? "1" : "0";
            result += HasAdditionalProcent ? "1" : "0";

            result += " ; ";
            result += AdditionalProcent;

            return result;
        }

    }
}