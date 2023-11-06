using System;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class TrustAccountBalanceExt
    {
        public static TrustAccountBalanceOnDate GetBalancesOfEachAssetOfAccount(this KeeperDataModel dataModel,
            TrustAccount trustAccount, DateTime upToDate)
        {
            var result = new TrustAccountBalanceOnDate() { Date = DateTime.MinValue };

            foreach (var tran in dataModel.InvestTranModels
                         .Where(t => t.TrustAccount == trustAccount && t.Timestamp.Date <= upToDate))
            {
                result.ApplyTran(dataModel, tran);
            }

            foreach (var asset in result.Assets)
            {
                if (asset.Quantity > 0)
                {
                    asset.CurrentPriceOfOne = dataModel.AssetRates.Last(r => r.TickerId == asset.InvestmentAssetId && r.Date <= upToDate).Value;
                    asset.CurrentPrice = asset.CurrentPriceOfOne * asset.Quantity;
                    var investmentAssetModel = dataModel.InvestmentAssets.First(a => a.Id == asset.InvestmentAssetId);
                    asset.AccumulatedCouponOfOne = investmentAssetModel.GetAccumulatedCoupon(upToDate);
                    asset.AccumulatedCoupon = asset.AccumulatedCouponOfOne * asset.Quantity;
                }
               
                var current = asset.CurrentPrice + asset.AccumulatedCoupon;
                var paid = asset.Price + asset.PaidCoupon;
                var sold = asset.SoldPrice + asset.SoldCoupon;
                asset.FinResult = current + sold - paid + asset.ReceivedCoupon - asset.BuySellFeeInTrustCurrency;
            }

            result.SetTotals();

            return result;
        }

        private static void SetTotals(this TrustAccountBalanceOnDate result)
        {
            result.OperationsFee = result.Assets.Sum(a => a.BuySellFeeInTrustCurrency);
            result.AllPaidFees = result.OperationsFee + result.BaseFee;
            result.Externals = result.TopUp + result.AllPaidFees - result.Withdraw;
            result.AllCurrentActives = result.Assets.Sum(a => a.CurrentPrice) + result.Assets.Sum(a => a.AccumulatedCoupon);
            result.FinResult = result.AllCurrentActives + result.Cash - result.Externals;
        }

        private static void ApplyTran(this TrustAccountBalanceOnDate result, KeeperDataModel dataModel, TrustTranModel tran)
        {
            switch (tran.InvestOperationType)
            {
                case InvestOperationType.TopUpTrustAccount:
                    result.TopUpTrustAccount(tran);
                    break;
                case InvestOperationType.BuyBonds:
                case InvestOperationType.BuyStocks:
                    result.BuyAsset(tran, dataModel);
                    break;
                case InvestOperationType.EnrollCouponOrDividends:
                    result.EnrollCouponOrDividends(tran);
                    break;
                case InvestOperationType.SellBonds:
                case InvestOperationType.SellStocks:
                    result.SellAsset(tran, dataModel);
                    break;
                case InvestOperationType.WithdrawFromTrustAccount:
                    result.WithdrawFromTrustAccount(tran);
                    break;
                case InvestOperationType.PayBaseCommission:
                    result.PayBaseCommission(dataModel, tran);
                    break;
                case InvestOperationType.PayBuySellFee:
                case InvestOperationType.PayWithdrawalTax:
                    break;
            }
        }

        private static void PayBaseCommission(this TrustAccountBalanceOnDate result, KeeperDataModel dataModel, TrustTranModel tran)
        {
            if (tran.TrustAccount.Currency == CurrencyCode.RUB)
            {
                var rate = dataModel.GetRate(tran.Timestamp, CurrencyCode.RUB);
                result.BaseFee += tran.CurrencyAmount * (decimal)(rate.Unit / rate.Value);
            }
            else
                result.BaseFee +=
                    dataModel.AmountInUsd(tran.Timestamp, CurrencyCode.BYN, tran.CurrencyAmount);
        }
        private static void WithdrawFromTrustAccount(this TrustAccountBalanceOnDate result, TrustTranModel tran)
        {
            result.Cash -= tran.CurrencyAmount;
            result.Withdraw += tran.CurrencyAmount;
        }
        private static void TopUpTrustAccount(this TrustAccountBalanceOnDate result, TrustTranModel tran)
        {
            result.Cash += tran.CurrencyAmount;
            result.TopUp += tran.CurrencyAmount;
        }

        private static void SellAsset(this TrustAccountBalanceOnDate result, TrustTranModel tran, KeeperDataModel dataModel)
        {
            result.Cash += tran.CurrencyAmount + tran.CouponAmount;
            var asset = result.Assets.First(t => t.InvestmentAssetId == tran.Asset.Id);

            asset.Quantity -= tran.AssetAmount;
            asset.SoldPrice += tran.CurrencyAmount;
            asset.SoldCoupon += tran.CouponAmount;

            result.OperationFee(asset, tran, dataModel);
        }

        private static void BuyAsset(this TrustAccountBalanceOnDate result, TrustTranModel tran, KeeperDataModel dataModel)
        {
            result.Cash -= tran.CurrencyAmount + tran.CouponAmount;
            var asset = result.Assets.FirstOrDefault(t => t.InvestmentAssetId == tran.Asset.Id);
            if (asset == null)
            {
                asset = new InvestmentAssetOnDate(tran);
                result.Assets.Add(asset);
            }

            asset.Quantity += tran.AssetAmount;
            asset.Price += tran.CurrencyAmount;
            asset.PaidCoupon += tran.CouponAmount;

            result.OperationFee(asset, tran, dataModel);
        }

        private static void OperationFee(this TrustAccountBalanceOnDate result, InvestmentAssetOnDate asset, TrustTranModel tran, KeeperDataModel dataModel)
        {
            if (tran.FeePaymentOperationId != 0)
            {
                asset.BuySellFee += tran.BuySellFee;
                asset.BuySellFeeCurrency = tran.BuySellFeeCurrency;
                if (tran.BuySellFeeCurrency == CurrencyCode.USD)
                    asset.BuySellFeeInTrustCurrency += tran.BuySellFee;
                else
                {
                    var feeTran = dataModel.InvestTranModels.First(t => t.Id == tran.FeePaymentOperationId);
                    asset.BuySellFeeInTrustCurrency += tran.TrustAccount.Currency == CurrencyCode.USD
                        ? dataModel.AmountInUsd(feeTran.Timestamp.Date, tran.BuySellFeeCurrency, tran.BuySellFee)
                        : (decimal)dataModel.GetRubBynRate(feeTran.Timestamp) * tran.BuySellFee;
                }
            }
            else
            {
                result.NotPaidFees += tran.BuySellFee;
            }
        }

        private static void EnrollCouponOrDividends(this TrustAccountBalanceOnDate result, TrustTranModel tran)
        {
            result.Cash += tran.CurrencyAmount;
            result.ReceivedCoupon += tran.CurrencyAmount;
            result.Assets.First(t => t.InvestmentAssetId == tran.Asset.Id).ReceivedCoupon += tran.CurrencyAmount;
        }
    }
}