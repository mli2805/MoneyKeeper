using System;
using System.Globalization;

namespace KeeperDomain
{
    [Serializable]
    public class TrustAsset : IDumpable, IParsable<TrustAsset>
    {
        public int Id { get; set; }

        public int TrustAccountId { get; set; }
        public string Ticker { get; set; }
        public string Title { get; set; }
        public StockMarket StockMarket { get; set; }
        public AssetType AssetType { get; set; }

        #region Bonds special properties

        public decimal Nominal { get; set; }
        public CalendarPeriod BondCouponPeriod { get; set; } = new CalendarPeriod();
        public double CouponRate { get; set; } // if fixed and known
        public DateTime PreviousCouponDate { get; set; }
        public DateTime BondExpirationDate { get; set; } = DateTime.MaxValue;

        #endregion

        public string Comment { get; set; } = string.Empty;

        public string Dump()
        {
            return Id + " ; " + TrustAccountId + " ; " + Ticker.Trim() + " ; " + Title.Trim() + " ; " + 
                   StockMarket + " ; " + AssetType + " ; " +
                   Nominal + " ; " + BondCouponPeriod.Dump() + " ; " +
                   CouponRate.ToString(new CultureInfo("en-US")) + " ; " + PreviousCouponDate.ToString("dd/MM/yyyy") + " ; " + 
                   BondExpirationDate.ToString("dd/MM/yyyy") + " ; " + Comment.Trim();
        }

        public TrustAsset FromString(string s)
        {
            var substrings = s.Split(';');
            Id = int.Parse(substrings[0]);
            TrustAccountId = int.Parse(substrings[1]);
            Ticker = substrings[2].Trim();
            Title = substrings[3].Trim();
            StockMarket = (StockMarket)Enum.Parse(typeof(StockMarket), substrings[4]);
            AssetType = (AssetType)Enum.Parse(typeof(AssetType), substrings[5]);
            Nominal = decimal.Parse(substrings[6], new CultureInfo("en-US"));
            BondCouponPeriod = CalendarPeriod.Parse(substrings[7]);
            CouponRate = double.Parse(substrings[8], new CultureInfo("en-US"));
            PreviousCouponDate = DateTime.ParseExact(substrings[9].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            BondExpirationDate = DateTime.ParseExact(substrings[10].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            Comment = substrings[11].Trim();
            return this;
        }
    }
}