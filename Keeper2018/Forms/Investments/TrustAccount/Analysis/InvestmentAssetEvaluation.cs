namespace Keeper2018
{
    public class InvestmentAssetEvaluation
    {
        public int InvestmentAssetId { get; }
        public string InvestmentAssetTicker { get; }
        public int Quantity { get; set; }
        public decimal PriceOfOne { get; set; }
        public decimal Price { get; set; }

        public decimal BuySellFeeByn { get; set; }
        public decimal BuySellFee { get; set; }
        public string FeeForDataGrid { get; set; }

        public decimal PaidCoupon { get; set; }
        public decimal ReceivedCoupon { get; set; }

        public decimal CurrentPriceOfOne { get; set; }
        public decimal CurrentPrice { get; set; }

        public InvestmentAssetEvaluation(InvestTranModel tran)
        {
            InvestmentAssetId = tran.Asset.Id;
            InvestmentAssetTicker = tran.Asset.Ticker;
            Quantity = tran.AssetAmount;
            Price = tran.CurrencyAmount;
            PaidCoupon = tran.CouponAmount;
        }

        public InvestmentAssetEvaluation ShallowCopy()
        {
            return (InvestmentAssetEvaluation)MemberwiseClone();
        }
    }
}