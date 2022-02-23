using System.Windows.Media;
using KeeperDomain;

namespace Keeper2018
{
    public static class TrustAccountLineProvider
    {
        public static TrustAccountLine Create(this InvestTranModel tran, decimal balanceBefore)
        {
            var line = new TrustAccountLine { Tran = tran, Title = tran.GetTransTitle() };

            switch (tran.InvestOperationType)
            {
                case InvestOperationType.BuyBonds:
                    line.BalanceAfter = balanceBefore - (tran.CurrencyAmount + tran.CouponAmount);
                    line.AmountOut = $"{tran.CurrencyAmount + tran.CouponAmount:#,0.00} {tran.Currency.ToString().ToLowerInvariant()}";
                    break;
                case InvestOperationType.BuyStocks:
                    line.BalanceAfter = balanceBefore - tran.CurrencyAmount;
                    line.AmountOut = tran.CurrencyAmountForDatagrid;
                    break;
                case InvestOperationType.EnrollCouponOrDividends:
                case InvestOperationType.TopUpTrustAccount:
                    line.BalanceAfter = balanceBefore + tran.CurrencyAmount;
                    line.AmountIn = tran.CurrencyAmountForDatagrid;
                    break;
                case InvestOperationType.PayBaseCommission:
                    line.BalanceAfter = balanceBefore;
                    line.AmountOut = tran.CurrencyAmountForDatagrid;
                    break;
            }

            line.TransBrush = GetTransBrush(tran);
            return line;
        }

        private static string GetTransTitle(this InvestTranModel tran)
        {
            var detail = "";
            if (tran.InvestOperationType == InvestOperationType.BuyBonds
                || tran.InvestOperationType == InvestOperationType.BuyStocks)
            {
                detail = $"{tran.Asset.Ticker} {tran.AssetAmount} шт";
            }
            return $"{tran.InvestOperationType.GetRussian()} {detail}";
        }

        private static Brush GetTransBrush(this InvestTranModel tran)
        {
            switch (tran.InvestOperationType)
            {
                case InvestOperationType.BuyBonds:
                case InvestOperationType.BuyStocks:
                    return Brushes.Red;
                case InvestOperationType.EnrollCouponOrDividends:
                    return Brushes.Blue;
                case InvestOperationType.PayBaseCommission:
                    return Brushes.Gray;
                case InvestOperationType.TopUpTrustAccount:
                default:
                    return Brushes.Black;
            }
        }
    }
}