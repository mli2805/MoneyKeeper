using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class TrustAccountBalanceOnDate
    {
        public DateTime Date;
        public decimal Cash;
        public Dictionary<InvestmentAsset, int> Assets = new Dictionary<InvestmentAsset, int>();

        public TrustAccountBalanceOnDate()
        {
        }

        public TrustAccountBalanceOnDate(TrustAccountBalanceOnDate source)
        {
            Date = source.Date;
            Cash = source.Cash;
            Assets = new Dictionary<InvestmentAsset, int>(source.Assets);
        }
    }

    public static class LiquidationValueExt
    {
        public static decimal GetLiquidationValue(this KeeperDataModel dataModel, TrustAccount trustAccount,
            DateTime lastDayOfPreviousPeriod, DateTime lastDayOfCurrentPeriod)
        {
            var balanceEmpty= new TrustAccountBalanceOnDate() { Date = DateTime.MinValue };
            var balanceBefore = dataModel.BuildUpTrustAccountBalance(trustAccount, 
                DateTime.MinValue, balanceEmpty, lastDayOfPreviousPeriod);
            var balanceNow = dataModel.BuildUpTrustAccountBalance(trustAccount,
                lastDayOfPreviousPeriod, balanceBefore, lastDayOfCurrentPeriod);
           
            return 0;
        }

        public static TrustAccountBalanceOnDate BuildUpTrustAccountBalance(this KeeperDataModel dataModel,
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
                        result.Cash -= tran.CurrencyAmount + tran.CouponAmount;
                        if (result.Assets.ContainsKey(tran.Asset))
                            result.Assets[tran.Asset] += tran.AssetAmount;
                        else
                            result.Assets.Add(tran.Asset, tran.AssetAmount);
                        break;
                    case InvestOperationType.EnrollCouponOrDividends:
                        result.Cash += tran.CurrencyAmount;
                        break;
                    case InvestOperationType.SellBonds:
                    case InvestOperationType.SellStocks:
                        result.Cash += tran.CurrencyAmount + tran.CouponAmount;
                        result.Assets[tran.Asset] -= tran.AssetAmount;
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
    }

    public class InvestmentAnalysisViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;

        public InvestmentAnalysisViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Initialize() { }
    }
}
