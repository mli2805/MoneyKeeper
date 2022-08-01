using System;
using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public class AssetsTableViewModel
    {
        public List<InvestmentAssetOnDate> Rows { get; set; } = new List<InvestmentAssetOnDate>();
        public InvestmentAssetOnDate Total { get; set; }

        public void Initialize(TrustAccountBalanceOnDate bal, KeeperDataModel dataModel, TrustAccount trustAccount, DateTime date, bool open)
        {
            foreach (var asset in bal.Assets)
            {
                asset.CurrentPriceOfOne = dataModel.AssetRates.Last(r => r.TickerId == asset.InvestmentAssetId).Value;
                asset.CurrentPrice = asset.CurrentPriceOfOne * asset.Quantity;
                var investmentAssetModel = dataModel.InvestmentAssets.First(a => a.Id == asset.InvestmentAssetId);
                asset.AccumulatedCouponOfOne = investmentAssetModel.GetAccumulatedCoupon(date);
                asset.AccumulatedCoupon = asset.AccumulatedCouponOfOne * asset.Quantity;
                asset.FinResult = asset.CurrentPrice + asset.AccumulatedCoupon + 
                    asset.SoldPrice - asset.Price - asset.PaidCoupon + asset.ReceivedCoupon - asset.BuySellFeeInTrustCurrency;
            }
            Rows.Clear();
            Rows.AddRange(open
                ? bal.Assets.Where(p => p.Quantity > 0)
                : bal.Assets.Where(p => p.Quantity == 0)
                );
            SummarizeTable(trustAccount);
        }

        private void SummarizeTable(TrustAccount trustAccount)
        {
            Total = new InvestmentAssetOnDate
            {
                InvestmentCurrency = trustAccount.Currency,
                PaidCoupon = Rows.Sum(r => r.PaidCoupon),
                Price = Rows.Sum(r => r.Price) + Rows.Sum(r => r.PaidCoupon),
                BuySellFee = Rows.Sum(r => r.BuySellFee),
                BuySellFeeCurrency = CurrencyCode.BYN,
                BuySellFeeInTrustCurrency = Rows.Sum(r => r.BuySellFeeInTrustCurrency),
                ReceivedCoupon = Rows.Sum(r => r.ReceivedCoupon),
                CurrentPrice = Rows.Sum(r => r.CurrentPrice) + Rows.Sum(r => r.AccumulatedCoupon),
                AccumulatedCoupon = Rows.Sum(r => r.AccumulatedCoupon),
                SoldPrice = Rows.Sum(r => r.SoldPrice) + Rows.Sum(r => r.SoldCoupon),
                FinResult = Rows.Sum(r => r.FinResult)
            };
        }
    }
}
