using System;
using KeeperDomain;

namespace Keeper2018
{
    public static class BuySellFeeCalculator
    {
        public static Tuple<decimal, CurrencyCode> EvaluatePurchaseFee(this InvestTranModel tran)
        {
            decimal purchaseFee = 0;
            switch (tran.TrustAccount.StockMarket)
            {
                case StockMarket.Russia:
                {
                    if (tran.InvestOperationType == InvestOperationType.BuyStocks)
                        purchaseFee = Math.Max((decimal)0.0015 * tran.CurrencyAmount, 8);
                    if (tran.InvestOperationType == InvestOperationType.BuyBonds)
                        purchaseFee = Math.Max((decimal)0.0010 * tran.CurrencyAmount, 8);
                    break;
                }
                case StockMarket.Usa:
                {
                    if (tran.CurrencyAmount / tran.AssetAmount <= 3)
                        purchaseFee = Math.Max((decimal)0.07 * tran.AssetAmount, 18);
                    else
                        purchaseFee = Math.Max((decimal)0.14 * tran.AssetAmount, 18);
                    break;
                }
            }

            return new Tuple<decimal, CurrencyCode>(purchaseFee, CurrencyCode.BYN);
        }
    }
}