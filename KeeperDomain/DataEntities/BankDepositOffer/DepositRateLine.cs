using System;
using System.Globalization;

namespace KeeperDomain
{
    [Serializable]
    public class DepositRateLine : IDumpable, IParsable<DepositRateLine>
    {
        public int Id { get; set; } //PK

        public int DepositOfferConditionsId { get; set; }

        public DateTime DateFrom { get; set; }
        public decimal AmountFrom { get; set; }
        public decimal AmountTo { get; set; }
        public decimal Rate { get; set; }

        public string Dump()
        {
            return Id  + " ; " + DepositOfferConditionsId  + " ; " + 
                   $"{DateFrom:dd/MM/yyyy}" + " ; " + AmountFrom.ToString(new CultureInfo("en-US")) + " ; "
                   + AmountTo.ToString(new CultureInfo("en-US")) + " ; " + Rate.ToString(new CultureInfo("en-US"));
        }

        public DepositRateLine FromString(string s)
        { var substrings = s.Split(';'); 
            Id = int.Parse(substrings[0].Trim());
            DepositOfferConditionsId = int.Parse(substrings[1].Trim());
            DateFrom = DateTime.ParseExact(substrings[2].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            AmountFrom = Convert.ToDecimal(substrings[3], new CultureInfo("en-US"));
            AmountTo = Convert.ToDecimal(substrings[4], new CultureInfo("en-US"));
            Rate = Convert.ToDecimal(substrings[5], new CultureInfo("en-US"));
            return this;
        }
    }
}