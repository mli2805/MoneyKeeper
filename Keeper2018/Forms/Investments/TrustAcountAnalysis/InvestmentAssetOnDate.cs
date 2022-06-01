using KeeperDomain;

namespace Keeper2018
{
    public class InvestmentAssetOnDate
    {
        public int InvestmentAssetId { get; }
        public string InvestmentAssetTicker { get; }
        public CurrencyCode InvestmentCurrency { get; set; }
        public int Quantity { get; set; }
        public decimal PriceOfOne => Price / Quantity;
        public decimal Price { get; set; }

        public decimal BuySellFee { get; set; }
        public CurrencyCode BuySellFeeCurrency { get; set; }
        public decimal BuySellFeeInTrustCurrency { get; set; }
        public string FeeForDataGrid => 
            $"{BuySellFeeInTrustCurrency:N} {InvestmentCurrency.ToString().ToLower()}  " +
                $"({BuySellFee} {BuySellFeeCurrency.ToString().ToLower()})";

        public decimal PaidCoupon { get; set; }
        public decimal ReceivedCoupon { get; set; }

        public decimal CurrentPriceOfOne { get; set; }
        public decimal CurrentPrice { get; set; }

        public decimal FinResult { get; set; }
        public decimal FinPercent => FinResult / Price * 100;
        public string FinPercentStr => $"{FinPercent:N}%";

        public InvestmentAssetOnDate()
        {
        }

        public InvestmentAssetOnDate(InvestTranModel tran)
        {
            InvestmentAssetId = tran.Asset.Id;
            InvestmentAssetTicker = tran.Asset.Ticker;
            InvestmentCurrency = tran.Currency;
        }

        public InvestmentAssetOnDate ShallowCopy()
        {
            return (InvestmentAssetOnDate)MemberwiseClone();
        }
    }
}