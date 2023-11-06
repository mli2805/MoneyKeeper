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
    public class DepositConditions : IDumpable, IParsable<DepositConditions>
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

        public bool HasAdditionalPercent { get; set; }
        public double AdditionalPercent { get; set; }

        public string Comment { get; set; }


        public string Dump()
        {
            return Id + " ; " + DepositOfferId + " ; " + $"{DateFrom:dd/MM/yyyy}"  + " ; " + RateFormula  + " ; " + 
                   IsFactDays + " ; " + EveryStartDay + " ; " + EveryFirstDayOfMonth + " ; " + 
                   EveryLastDayOfMonth + " ; " + EveryNDays + " ; " + NDays + " ; " + 
                   IsCapitalized + " ; " + 
                   HasAdditionalPercent + " ; " + AdditionalPercent.ToString(new CultureInfo("en-US")) + " ; " + 
                   Comment;
        }

        public DepositConditions FromString(string s)
        {
            var substrings = s.Split(';');
            Id = int.Parse(substrings[0]);
            DepositOfferId = int.Parse(substrings[1]);
            DateFrom = DateTime.ParseExact(substrings[2].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);

            RateFormula = substrings[3].Trim();
            IsFactDays = bool.Parse(substrings[4]);
            EveryStartDay = bool.Parse(substrings[5]);
            EveryFirstDayOfMonth = bool.Parse(substrings[6]);
            EveryLastDayOfMonth = bool.Parse(substrings[7]);

            EveryNDays = bool.Parse(substrings[8]);
            NDays = int.Parse(substrings[9]);

            IsCapitalized = bool.Parse(substrings[10]);
            HasAdditionalPercent = bool.Parse(substrings[11]);
            AdditionalPercent = double.Parse(substrings[12]);

            Comment = substrings[13].Trim();
            return this;
        }
    }
}