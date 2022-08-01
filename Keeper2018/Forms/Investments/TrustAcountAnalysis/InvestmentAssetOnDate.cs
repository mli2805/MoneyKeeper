using KeeperDomain;

namespace Keeper2018
{
    public class InvestmentAssetOnDate
    {
        public int InvestmentAssetId { get; }
        public string InvestmentAssetTicker { get; }
        public CurrencyCode InvestmentCurrency { get; set; }
        public int Quantity { get; set; }
        public decimal PriceOfOne => Quantity != 0 ? Price / Quantity : 0;
        public decimal Price { get; set; } // добавляем сумму при покупку
        public decimal PaidCoupon { get; set; } // купон заплачен при покупке
        public string PriceStr => PaidCoupon > 0 
            ? $"{Price:N} + {PaidCoupon:N} = {Price + PaidCoupon:N}"
            : $"{Price:N}";
        public decimal SoldPrice { get; set; } // добавляем сумму при продаже
        public decimal SoldCoupon { get; set; } // купон получен при продаже

        public string SoldPriceStr => SoldCoupon > 0
            ? $"{SoldPrice:N} + {SoldCoupon:N} = {SoldPrice + SoldCoupon:N}"
            : $"{SoldPrice:N}";

      
        public decimal ReceivedCoupon { get; set; } // купон зачисленный отдельно


        public decimal CurrentPriceOfOne { get; set; }
        public decimal AccumulatedCouponOfOne { get; set; }
        public string CurrentPriceOfOneStr => AccumulatedCouponOfOne > 0
            ? $"{CurrentPriceOfOne:0,0.00##} + {AccumulatedCouponOfOne:0.00} = {CurrentPriceOfOne + AccumulatedCouponOfOne:N}"
            : $"{CurrentPriceOfOne:N}";

        public decimal CurrentPrice { get; set; }
        public decimal AccumulatedCoupon { get; set; }
        public string CurrentPriceStr => AccumulatedCoupon > 0
            ? $"{CurrentPrice:0,0.00} + {AccumulatedCoupon:0.00} = {CurrentPrice + AccumulatedCoupon:N}"
            : $"{CurrentPrice:N}";

        public decimal BuySellFee { get; set; }
        public CurrencyCode BuySellFeeCurrency { get; set; }
        public decimal BuySellFeeInTrustCurrency { get; set; }
        public string FeeForDataGrid => BuySellFee == 0 ? "" :
            $"{BuySellFeeInTrustCurrency:N} {InvestmentCurrency.ToString().ToLower()}  " +
            $"({BuySellFee} {BuySellFeeCurrency.ToString().ToLower()})";

        public decimal FinResult { get; set; }
        public string FinResultStr => $"{FinResult:N}  ({FinResult / Price * 100:N}%)";

        public InvestmentAssetOnDate()
        {
        }

        public InvestmentAssetOnDate(InvestTranModel tran)
        {
            InvestmentAssetId = tran.Asset.Id;
            InvestmentAssetTicker = tran.Asset.Ticker;
            InvestmentCurrency = tran.Currency;
        }

    }
}