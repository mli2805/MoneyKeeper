using System;
using System.Globalization;

namespace KeeperDomain
{
    [Serializable]
    public class InvestmentAsset
    {
        public int Id { get; set; }

        public int TrustAccountId { get; set; }
        public string Ticker { get; set; }
        public string Title { get; set; }
        public StockMarket StockMarket { get; set; }
        public AssetType AssetType { get; set; }

        public decimal Nominal { get; set; }
        public int BondCouponPeriodDays { get; set; }
        public CouponPeriod BondCouponPeriod { get; set; } = new CouponPeriod();
        public double CouponRate { get; set; } // if fixed and known
        public DateTime BondExpirationDate { get; set; } = DateTime.MaxValue; // if Bond not Stack

        public string Comment { get; set; } = string.Empty;

        public string Dump()
        {
            return Id + " ; " + TrustAccountId + " ; " + Ticker.Trim() + " ; " + Title.Trim() + " ; " + 
                   StockMarket + " ; " + AssetType + " ; " +
                   Nominal + " ; " + BondCouponPeriod.Dump() + " ; " +
                   CouponRate.ToString(new CultureInfo("en-US")) + " ; " + 
                   BondExpirationDate.ToString("dd/MM/yyyy") + " ; " + Comment.Trim();
        }
    }
}