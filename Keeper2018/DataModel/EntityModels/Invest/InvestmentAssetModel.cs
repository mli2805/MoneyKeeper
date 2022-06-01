using System;
using KeeperDomain;

namespace Keeper2018
{
    public class InvestmentAssetModel
    {
        public int Id { get; set; }

        public TrustAccount TrustAccount { get; set; }
        public string Ticker { get; set; }
        public string Title { get; set; }
        public AssetType AssetType { get; set; }

        public double CouponRate { get; set; } // if fixed and known
        public DateTime BondExpirationDate { get; set; } = DateTime.MaxValue; // if Bond not Stack

        public string Comment { get; set; }
    }
}
