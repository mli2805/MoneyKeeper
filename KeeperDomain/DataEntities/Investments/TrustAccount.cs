using System;

namespace KeeperDomain
{
    [Serializable]
    public class TrustAccount : IDumpable, IParsable<TrustAccount>
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public StockMarket StockMarket { get; set; }
        public string Number { get; set; }
        public CurrencyCode Currency { get; set; }
        public int AccountId { get; set; }
        public string Comment { get; set; }

        public string ToCombo => Title + " - " + Number;

        public string Dump()
        {
            return Id + " ; " + Title + " ; " + StockMarket + " ; " + Number + " ; " + Currency + " ; " + AccountId + " ; " + Comment;
        }

        public TrustAccount FromString(string s)
        {
            var substrings = s.Split(';');
            Id = int.Parse(substrings[0]);
            Title = substrings[1].Trim();
            StockMarket = (StockMarket)Enum.Parse(typeof(StockMarket), substrings[2]);
            Number = substrings[3].Trim();
            Currency = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[4]);
            AccountId = int.Parse(substrings[5].Trim());
            Comment = substrings[6].Trim();
            return this;
        }
    }
}
