using System.Collections.Generic;

namespace Keeper2018
{
    public class InvestmentAssetState
    {
        public InvestmentAssetModel Asset { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; } // price + coupon

        public decimal PriceInUsd { get; set; } // price + coupon
        public decimal AveragePrice => Price / Quantity;

        public List<InvestTranModel> Trans { get; set; } = new List<InvestTranModel>();

        public InvestmentAssetState()
        {
        }

        public InvestmentAssetState(InvestmentAssetModel asset)
        {
            Asset = asset;
        }

        public InvestmentAssetState(InvestmentAssetState before)
        {
            Asset = before.Asset;
            Quantity = before.Quantity;
            Price = before.Price;
        }
    }

    public class InvestmentAssetOnPeriod
    {
        public InvestmentAssetState Initial { get; set; }
        public InvestmentAssetState Start { get; set; }
        public InvestmentAssetState End { get; set; }
    }
}
