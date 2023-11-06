using System.Windows.Media;
using KeeperDomain;

namespace Keeper2018
{
    public static class TrustStatisticsLineProvider
    {
        public static TrustStatisticsLine Create(this TrustTranModel tran, decimal balanceBefore, bool isForWholeTrustAccount)
        {
            var line = new TrustStatisticsLine { Tran = tran, Title = tran.GetTransTitle(), IsForWholeTrustAccount = isForWholeTrustAccount };

            switch (tran.InvestOperationType)
            {
                case InvestOperationType.BuyBonds:
                    line.AmountOut = tran.FullAmountForDatagrid;
                    line.BynFee = tran.BuySellFeeForDataGrid;
                    break;
                case InvestOperationType.BuyStocks:
                    line.AmountOut = tran.CurrencyAmountForDatagrid;
                    line.BynFee = tran.BuySellFeeForDataGrid;
                    break;
               case InvestOperationType.SellBonds:
                    line.AmountOut = tran.FullAmountForDatagrid;
                    line.BynFee = tran.BuySellFeeForDataGrid;
                    break;
                case InvestOperationType.SellStocks:
                    line.AmountOut = tran.CurrencyAmountForDatagrid;
                    line.BynFee = tran.BuySellFeeForDataGrid;
                    break;
                case InvestOperationType.EnrollCouponOrDividends:
                case InvestOperationType.TopUpTrustAccount:
                    line.AmountIn = tran.CurrencyAmountForDatagrid;
                    break;
                case InvestOperationType.PayBuySellFee:
                case InvestOperationType.PayBaseCommission:
                    line.BynFee = tran.CurrencyAmountForDatagrid;
                    break;
                case InvestOperationType.WithdrawFromTrustAccount:
                    line.AmountOut = tran.CurrencyAmountForDatagrid;
                    break;
            }

            line.BalanceAfter = balanceBefore + (isForWholeTrustAccount ? tran.GetBalanceChange() : tran.GetQuantityChange());
            line.TransBrush = GetTransBrush(tran);
            return line;
        }

        private static decimal GetQuantityChange(this TrustTranModel tran)
        {
            switch (tran.InvestOperationType)
            {
                case InvestOperationType.BuyBonds:
                case InvestOperationType.BuyStocks:
                    return tran.AssetAmount;
                case InvestOperationType.SellBonds:
                case InvestOperationType.SellStocks:
                    return -tran.AssetAmount;
                default:
                    return 0;
            }
        }

        private static decimal GetBalanceChange(this TrustTranModel tran)
        {
            switch (tran.InvestOperationType)
            {
                case InvestOperationType.SellStocks:
                    return tran.CurrencyAmount;
                case InvestOperationType.SellBonds:
                    return tran.CurrencyAmount + tran.CouponAmount;
                case InvestOperationType.BuyBonds:
                    return - (tran.CurrencyAmount + tran.CouponAmount);
                case InvestOperationType.BuyStocks:
                case InvestOperationType.WithdrawFromTrustAccount:
                    return - tran.CurrencyAmount;
                case InvestOperationType.EnrollCouponOrDividends:
                case InvestOperationType.TopUpTrustAccount:
                    return tran.CurrencyAmount;
                case InvestOperationType.PayBuySellFee:
                case InvestOperationType.PayBaseCommission:
                default:
                    return 0;
            }
        }

        private static string GetTransTitle(this TrustTranModel tran)
        {
            var result = $"{tran.InvestOperationType.GetRussian()}";
            if (tran.InvestOperationType == InvestOperationType.BuyBonds
                || tran.InvestOperationType == InvestOperationType.BuyStocks
                || tran.InvestOperationType == InvestOperationType.SellBonds
                || tran.InvestOperationType == InvestOperationType.SellStocks)
            {
                result += $" {tran.Asset.Ticker} {tran.AssetAmount} шт по {tran.CurrencyAmount / tran.AssetAmount:N}";
                if (tran.InvestOperationType == InvestOperationType.BuyBonds
                    || tran.InvestOperationType == InvestOperationType.SellBonds)
                    result += $" + {tran.CouponAmount / tran.AssetAmount:N}";
            } 
            else if (tran.InvestOperationType == InvestOperationType.EnrollCouponOrDividends)
            {
                result += $" по {tran.Asset.Ticker}";
            }
            return result;
        }

        private static Brush GetTransBrush(this TrustTranModel tran)
        {
            switch (tran.InvestOperationType)
            {
                case InvestOperationType.BuyBonds:
                case InvestOperationType.BuyStocks:
                    return Brushes.Red;
                case InvestOperationType.EnrollCouponOrDividends:
                    return Brushes.Blue;
                case InvestOperationType.PayWithdrawalTax:
                case InvestOperationType.PayBuySellFee:
                case InvestOperationType.PayBaseCommission:
                    return Brushes.Gray;
                case InvestOperationType.TopUpTrustAccount:
                case InvestOperationType.WithdrawFromTrustAccount:
                default:
                    return Brushes.Black;
            }
        }
    }
}