using System;
using System.Globalization;

namespace KeeperDomain
{
    [Serializable]
    public class CardBalanceMemo : IDumpable, IParsable<CardBalanceMemo>
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public decimal BalanceThreshold { get; set; }

        public decimal ExpenseNotLess { get; set; }
        public decimal ExpenseNotMore { get; set; }
        
        public string Comment { get; set; }

        public string Dump()
        {
            return Id + " ; " + AccountId + " ; " + 
                   BalanceThreshold.ToString(new CultureInfo("en-US")) + " ; " + 
                   ExpenseNotLess.ToString(new CultureInfo("en-US")) + " ; " + 
                   ExpenseNotMore.ToString(new CultureInfo("en-US")) + " ; " + 
                   Comment;
        }

        public CardBalanceMemo FromString(string s)
        {
            var substrings = s.Split(';');
            Id = int.Parse(substrings[0]);
            AccountId = int.Parse(substrings[1]);
            BalanceThreshold = Convert.ToDecimal(substrings[2], new CultureInfo("en-US"));
            ExpenseNotLess = Convert.ToDecimal(substrings[3], new CultureInfo("en-US"));
            ExpenseNotMore = Convert.ToDecimal(substrings[4], new CultureInfo("en-US"));
            Comment = substrings[5].Trim();
            return this;
        }
    }
}
