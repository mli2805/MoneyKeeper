using System;
using System.Globalization;
using KeeperDomain;

namespace Keeper2018
{
    public class InvestmentAssetModel
    {
        public int Id { get; set; }

        public TrustAccount TrustAccount { get; set; }
        public string Ticker { get; set; }
        public string Title { get; set; }
        public StockMarket StockMarket { get; set; }
        public AssetType AssetType { get; set; }

        public double CouponRate { get; set; } // if fixed and known
        public string CouponRateStr => AssetType == AssetType.Bond ? CouponRate.ToString("F1") : "";

        // if Bond not Stack
        public decimal Nominal { get; set; }
        public string NominalStr => Nominal == 0 
            ? "" 
            : Nominal.ToString(new CultureInfo("en-US")) + " руб";

        public int BondCouponPeriodDays { get; set; }
        public CouponPeriod BondCouponPeriod { get; set; } = new CouponPeriod();
        public DateTime PreviousCouponDate { get; set; }
        public DateTime BondExpirationDate { get; set; } = DateTime.MaxValue;
        public string BondExpirationDateStr => AssetType == AssetType.Bond ? BondExpirationDate.ToLongDateString() : "";

        public string Comment { get; set; }
    }
}
