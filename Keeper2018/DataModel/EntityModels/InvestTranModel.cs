using System;
using KeeperDomain;

namespace Keeper2018
{
   public class InvestTranModel
    {
        public int Id { get; set; }
        public InvestOperationType InvestOperationType { get; set; }
        public DateTime Timestamp { get; set; }

        public AccountModel AccountModel { get; set; }
        public TrustAccount TrustAccount { get; set; }

        public double CurrencyAmount { get; set; }
        public CurrencyCode Currency { get; set; }

        public double AssetAmount { get; set; }
        public InvestmentAsset Asset { get; set; }

        public string Comment { get; set; }
    }
}