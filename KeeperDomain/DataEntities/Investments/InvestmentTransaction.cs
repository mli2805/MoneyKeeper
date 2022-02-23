using System;
using System.Globalization;

namespace KeeperDomain
{
    [Serializable]
    public class InvestmentTransaction
    {
        public int Id { get; set; }
        public InvestOperationType InvestOperationType { get; set; }
        public DateTime Timestamp { get; set; }

        public int AccountId { get; set; }
        public int TrustAccountId { get; set; }

        public decimal CurrencyAmount { get; set; }
        public decimal CouponAmount { get; set; }
        public CurrencyCode Currency { get; set; }

        public int AssetAmount { get; set; }
        public int AssetId { get; set; }

        public string Comment { get; set; }

        public string Dump()
        {
            return Id + " ; " + InvestOperationType + " ; " + Timestamp.ToString("dd/MM/yyyy HH:mm") + " ; " + 
                   AccountId + " ; " + TrustAccountId + " ; " +
                   CurrencyAmount.ToString(new CultureInfo("en-US")) + " ; " + 
                   CouponAmount.ToString(new CultureInfo("en-US")) + " ; " + Currency + " ; " +
                   AssetAmount.ToString(new CultureInfo("en-US")) + " ; " + AssetId + " ; " + 
                   Comment;
        }
    }
}