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

        public void Initialize(TrustAccountBalanceOnDate bal, KeeperDataModel dataModel, TrustAccount trustAccount, DateTime date)
        {
            foreach (var asset in bal.Assets.Where(p=>p.Quantity > 0))
            {
                asset.CurrentPriceOfOne = dataModel.AssetRates.Last(r => r.TickerId == asset.InvestmentAssetId).Value;
                asset.CurrentPrice = asset.CurrentPriceOfOne * asset.Quantity;
                var investmentAssetModel = dataModel.InvestmentAssets.First(a => a.Id == asset.InvestmentAssetId);
                asset.AccumulatedCouponOfOne = investmentAssetModel.GetAccumulatedCoupon(date);
                asset.AccumulatedCoupon = asset.AccumulatedCouponOfOne * asset.Quantity;
                asset.FinResult = asset.CurrentPrice - asset.Price - asset.PaidCoupon + asset.ReceivedCoupon;
            }
            Rows.Clear();
            Rows.AddRange(bal.Assets.Where(p=>p.Quantity > 0));
            SummarizeTable(trustAccount);
        }

        private void SummarizeTable(TrustAccount trustAccount)
        {
            Total = new InvestmentAssetOnDate
            {
                InvestmentCurrency = trustAccount.Currency,
                Price = Rows.Sum(r => r.Price),
                BuySellFee = Rows.Sum(r => r.BuySellFee),
                BuySellFeeCurrency = CurrencyCode.BYN,
                BuySellFeeInTrustCurrency = Rows.Sum(r => r.BuySellFeeInTrustCurrency),
                PaidCoupon = Rows.Sum(r => r.PaidCoupon),
                ReceivedCoupon = Rows.Sum(r => r.ReceivedCoupon),
                AccumulatedCoupon = Rows.Sum(r => r.AccumulatedCoupon),
                CurrentPrice = Rows.Sum(r => r.CurrentPrice),
                FinResult = Rows.Sum(r => r.FinResult)
            };
        }
    }
}
