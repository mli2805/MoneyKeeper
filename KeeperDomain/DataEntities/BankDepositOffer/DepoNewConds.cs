using System;
using System.Globalization;

namespace KeeperDomain
{
    public enum RateType
    {
        Floating,
        Linked,
        Fixed,
    }

    [Serializable]
    public class DepoNewConds
    {
        public int Id { get; set; } //PK
        public int DepositOfferId { get; set; } 
        public DateTime DateFrom { get; set; }

        public string RateFormula { get; set; }
        public bool IsFactDays { get; set; } // true 28-31/365 false 30/360
        public bool EveryStartDay { get; set; }
        public bool EveryFirstDayOfMonth { get; set; } 
        public bool EveryLastDayOfMonth { get; set; }

        public bool EveryNDays { get; set; }
        public int NDays { get; set; }

        public bool IsCapitalized { get; set; }

        public bool HasAdditionalProcent { get; set; }
        public double AdditionalProcent { get; set; }

        public string Comment { get; set; }


        public string Dump()
        {
            return Id + " ; " + DepositOfferId + " ; " + $"{DateFrom:dd/MM/yyyy}"  + " ; " + RateFormula  + " ; " + 
                   IsFactDays + " ; " + EveryStartDay + " ; " + EveryFirstDayOfMonth + " ; " + 
                   EveryLastDayOfMonth + " ; " + EveryNDays + " ; " + NDays + " ; " + 
                   IsCapitalized + " ; " + 
                   HasAdditionalProcent + " ; " + AdditionalProcent.ToString(new CultureInfo("en-US")) + " ; " + 
                   Comment;
        }
    }
}