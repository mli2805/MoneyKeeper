using System;
using System.Globalization;

namespace KeeperDomain
{
    [Serializable]
    public class LargeExpenseThreshold : IDumpable, IParsable<LargeExpenseThreshold>
    {
        public int Id { get; set; }
        public DateTime FromDate { get; set; }
        public decimal Amount { get; set; } // for month analysis
        public decimal AmountForYearAnalysis { get; set; }


        public string Dump()
        {
            return Id + " ; " + 
                   FromDate.ToString("dd/MM/yyyy") + " ; " +
                   Amount.ToString(new CultureInfo("en-US")) + " ; " +
                   AmountForYearAnalysis.ToString(new CultureInfo("en-US"));
        }

        public LargeExpenseThreshold FromString(string s)
        {
            var substrings = s.Split(';');
            Id = int.Parse(substrings[0]);
            FromDate = DateTime.ParseExact(substrings[1].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            Amount = Convert.ToDecimal(substrings[2], new CultureInfo("en-US"));
            AmountForYearAnalysis = Convert.ToDecimal(substrings[3], new CultureInfo("en-US"));
            return this;
        }
    }
}