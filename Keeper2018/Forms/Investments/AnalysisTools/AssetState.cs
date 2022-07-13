using System.Collections.Generic;
using KeeperDomain;

namespace Keeper2018
{
    public class AssetState
    {
        public InvestmentAssetModel Asset { get; set; }

        public string Caption { get; set; }

        public int Quantity { get; set; }
        public decimal PriceWithoutCoupon { get; set; } // price
        public decimal Price { get; set; } // price + coupon

        public decimal PriceInUsd { get; set; } // price + coupon

        public string PriceStr => Asset.TrustAccount.Currency == CurrencyCode.USD
            ? $"{Price:N} usd"
            : $"{Price:N} {Asset.TrustAccount.Currency.ToString().ToLower()} ({PriceInUsd:N} usd)";

        public decimal AveragePrice => Quantity == 0 ? 0 : Price / Quantity;
        public decimal AveragePriceInUsd => Quantity == 0 ? 0 : PriceInUsd / Quantity;
        public string AveragePriceStr => Asset.TrustAccount.Currency == CurrencyCode.USD
            ? $"{AveragePrice:N} usd"
            : $"{AveragePrice:N} {Asset.TrustAccount.Currency.ToString().ToLower()} ({AveragePriceInUsd:N} usd)";


        public decimal ReceivedCoupon { get; set; }
        public decimal ReceivedCouponInUsd { get; set; }
        public string ReceivedCouponStr => Asset.TrustAccount.Currency == CurrencyCode.USD
            ? $"{ReceivedCoupon:N} usd"
            : $"{ReceivedCoupon:N} {Asset.TrustAccount.Currency.ToString().ToLower()} ({ReceivedCouponInUsd:N} usd)";

        public decimal OperationFees { get; set; }
        public decimal OperationFeesInUsd { get; set; }
        public string OperationFeesStr => $"{OperationFees:N} byn ({OperationFeesInUsd:N} usd)";

        public decimal CurrentCurrencyRate;
        public string CurrentCurrencyRateStr;
        public decimal CurrentAssetRate;
        public decimal AccumulatedCouponIncome;

        public List<InvestTranModel> Trans { get; set; } = new List<InvestTranModel>();

        public AssetState(InvestmentAssetModel asset)
        {
            Asset = asset;
        }

        public AssetState Clone(string newCaption)
        {
            var clone = (AssetState)MemberwiseClone();
            clone.Caption = newCaption;
            clone.Trans = new List<InvestTranModel>();
            clone.Trans.AddRange(Trans);
            return clone;
        }
    }
}
