using System;
using System.Globalization;
using KeeperDomain;

namespace Keeper2018
{
   public class InvestTranModel
    {
        public int Id { get; set; }
        public InvestOperationType InvestOperationType { get; set; }
        public string TypeForDatagrid => InvestOperationType.GetRussian();
        public DateTime Timestamp { get; set; }

        public AccountModel AccountModel { get; set; }
        public string AccountForDataGrid => AccountModel.Name;
        public TrustAccount TrustAccount { get; set; }

        public decimal CurrencyAmount { get; set; }
        public CurrencyCode Currency { get; set; }
        public string CurrencyAmountForDatagrid => $"{CurrencyAmount} {Currency.ToString().ToLowerInvariant()}";

        public double AssetAmount { get; set; }

        public string AssetAmountForDatagrid =>
            AssetAmount == 0 ? "" : AssetAmount.ToString(CultureInfo.CurrentCulture);
        public InvestmentAsset Asset { get; set; }

        public string Comment { get; set; }
    }
}