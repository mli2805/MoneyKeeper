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

        public string Dump()
        {
            return Id + " ; " + AccountId + " ; " + BalanceThreshold.ToString(new CultureInfo("en-US"));
        }

        public CardBalanceMemo FromString(string s)
        {
            var substrings = s.Split(';');
            Id = int.Parse(substrings[0]);
            AccountId = int.Parse(substrings[1]);
            BalanceThreshold = Convert.ToDecimal(substrings[2], new CultureInfo("en-US"));
            return this;
        }
    }
}
