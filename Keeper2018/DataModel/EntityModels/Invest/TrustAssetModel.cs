using System;
using System.Globalization;
using System.Windows;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class TrustAssetModel : PropertyChangedBase
    {
        public int Id { get; set; }

        public TrustAccount TrustAccount { get; set; }
        public string Ticker { get; set; }
        public string Title { get; set; }
        public StockMarket StockMarket { get; set; }

        private AssetType _assetType;
        public AssetType AssetType
        {
            get => _assetType;
            set
            {
                if (value == _assetType) return;
                _assetType = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(CouponRateStr));
                NotifyOfPropertyChange(nameof(BondExpirationDateStr));
                NotifyOfPropertyChange(nameof(BondPropertiesVisibility));
            }
        }

        public Visibility BondPropertiesVisibility => AssetType == AssetType.Bond ? Visibility.Visible : Visibility.Collapsed;

        public double CouponRate { get; set; } // if fixed and known
        public string CouponRateStr => AssetType == AssetType.Bond ? CouponRate.ToString("F1") + " %" : "";

        // if Bond not Stack
        public decimal Nominal { get; set; }
        public string NominalStr => Nominal == 0 
            ? "" 
            : Nominal.ToString(new CultureInfo("en-US")) + " руб";

        public CalendarPeriod BondCouponPeriod { get; set; } = new CalendarPeriod();
        public string BondCouponPeriodStr => AssetType == AssetType.Bond ? BondCouponPeriod.Dump() : "";
        public DateTime PreviousCouponDate { get; set; }
        public string PreviousCouponDateStr => AssetType == AssetType.Bond ? PreviousCouponDate.ToLongDateString() : "";
        public DateTime BondExpirationDate { get; set; } = DateTime.MaxValue;
        public string BondExpirationDateStr => AssetType == AssetType.Bond ? BondExpirationDate.ToLongDateString() : "";

        public string Comment { get; set; } = string.Empty;

        public TrustAssetModel Clone()
        {
            var clone = (TrustAssetModel)MemberwiseClone();
            clone.BondCouponPeriod = BondCouponPeriod.Clone();
            return clone;
        }

        public void CopyFrom(TrustAssetModel from)
        {
            Id = from.Id;
            TrustAccount = from.TrustAccount;
            Ticker = from.Ticker;
            Title = from.Title;
            StockMarket = from.StockMarket;
            AssetType = from.AssetType;
            CouponRate = from.CouponRate;
            Nominal = from.Nominal;
            BondCouponPeriod = from.BondCouponPeriod.Clone();
            PreviousCouponDate = from.PreviousCouponDate;
            BondExpirationDate = from.BondExpirationDate;
            Comment = from.Comment;
        }
    }
}
