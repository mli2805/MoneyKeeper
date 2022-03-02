using System;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class TrustAccountBalanceExt
    {
        public static TrustAccountBalanceOnDate GetTrustAccountBalance(this KeeperDataModel dataModel,
            TrustAccount trustAccount, DateTime upToDate)
        {
            var balanceEmpty = new TrustAccountBalanceOnDate() { Date = DateTime.MinValue };
            return dataModel.BuildUpTrustAccountBalance(trustAccount,
                DateTime.MinValue, balanceEmpty, upToDate);
        }

        public static decimal GetTwoBalances(this KeeperDataModel dataModel, TrustAccount trustAccount,
            DateTime lastDayOfPreviousPeriod, DateTime lastDayOfCurrentPeriod)
        {
            var balanceEmpty= new TrustAccountBalanceOnDate() { Date = DateTime.MinValue };
            var balanceBefore = dataModel.BuildUpTrustAccountBalance(trustAccount, 
                DateTime.MinValue, balanceEmpty, lastDayOfPreviousPeriod);
            var balanceNow = dataModel.BuildUpTrustAccountBalance(trustAccount,
                lastDayOfPreviousPeriod, balanceBefore, lastDayOfCurrentPeriod);
           
            return 0;
        }

        private static TrustAccountBalanceOnDate BuildUpTrustAccountBalance(this KeeperDataModel dataModel,
            TrustAccount trustAccount,
            DateTime lastDayOfPreviousPeriod, TrustAccountBalanceOnDate lastDayOfPreviousPeriodBalance,
            DateTime lastDayOfCurrentPeriod)
        {
            var result = new TrustAccountBalanceOnDate(lastDayOfPreviousPeriodBalance);

            foreach (var tran in dataModel.InvestTranModels.Where(t => t.TrustAccount == trustAccount
                                                                       && t.Timestamp.Date > lastDayOfPreviousPeriod &&
                                                                       t.Timestamp.Date <= lastDayOfCurrentPeriod))
            {
                switch (tran.InvestOperationType)
                {
                    case InvestOperationType.TopUpTrustAccount:
                        result.Cash += tran.CurrencyAmount;
                        break;
                    case InvestOperationType.BuyBonds:
                    case InvestOperationType.BuyStocks:
                        BuyAsset(result, tran);
                        break;
                    case InvestOperationType.EnrollCouponOrDividends:
                        result.Cash += tran.CurrencyAmount;
                        result.Assets.First(t=>t.InvestmentAssetId == tran.Asset.Id).ReceivedCoupon += tran.CurrencyAmount;
                        break;
                    case InvestOperationType.SellBonds:
                    case InvestOperationType.SellStocks:
                        SellAsset(result, tran);
                        break;
                    case InvestOperationType.WithdrawFromTrustAccount:
                        result.Cash -= tran.CurrencyAmount;
                        break;

                    case InvestOperationType.PayBaseCommission:
                    case InvestOperationType.PayBuySellFee:
                    case InvestOperationType.PayWithdrawalTax:
                        break;
                }
            }

            return result;
        }

        private static void SellAsset(TrustAccountBalanceOnDate result, InvestTranModel tran)
        {
            result.Cash += tran.CurrencyAmount + tran.CouponAmount;
            var asset2 = result.Assets.First(t => t.InvestmentAssetId == tran.Asset.Id);
            asset2.Quantity -= tran.AssetAmount;
            asset2.Price -= tran.CurrencyAmount;
            asset2.ReceivedCoupon += tran.CouponAmount;
        }

        private static void BuyAsset(TrustAccountBalanceOnDate result, InvestTranModel tran)
        {
            result.Cash -= tran.CurrencyAmount + tran.CouponAmount;
            var asset = result.Assets.FirstOrDefault(t => t.InvestmentAssetId == tran.Asset.Id);
            if (asset != null)
            {
                asset.Quantity += tran.AssetAmount;
                asset.Price += tran.CurrencyAmount;
                asset.PaidCoupon += tran.CouponAmount;
            }
            else
            {
                result.Assets.Add(new InvestmentAssetEvaluation(tran));
            }
        }
    }
}