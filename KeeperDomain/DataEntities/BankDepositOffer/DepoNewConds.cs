using System;
using System.Globalization;

namespace KeeperDomain
{
    [Serializable]
    public class DepoNewConds
    {
        public int Id { get; set; } //PK
        public int DepositOfferId { get; set; } 
        public DateTime DateFrom { get; set; }


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

        public string Comment { get; set; }


        public string Dump()
        {
            return Id + " ; " + DepositOfferId + " ; " + $"{DateFrom:dd/MM/yyyy}"  + " ; " + 
                   IsFactDays + " ; " + EveryStartDay + " ; " + EveryFirstDayOfMonth + " ; " + 
                   EveryLastDayOfMonth + " ; " + IsCapitalized + " ; " + IsRateFixed +" ; " + 
                   HasAdditionalProcent + " ; " + AdditionalProcent.ToString(new CultureInfo("en-US")) + " ; " + 
                   Comment;
        }
    }
}