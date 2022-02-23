using System;

namespace KeeperDomain
{
    [Serializable]
    public class TrustAccount
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public Market StockMarket { get; set; }
        public string Number { get; set; }
        public CurrencyCode Currency { get; set; }
        public int AccountId { get; set; }
        public string Comment { get; set; }

        public string ToCombo => Title + " - " + Number;

        public string Dump()
        {
            return Id + " ; " + Title + " ; " + StockMarket + " ; " + Number + " ; " + Currency + " ; " + AccountId + " ; " + Comment;
        }
    }
}
