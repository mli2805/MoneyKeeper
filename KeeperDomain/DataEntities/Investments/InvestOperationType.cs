namespace KeeperDomain
{
    public enum InvestOperationType
    {
        TopUpTrustAccount,

        BuyBonds,
        BuyStocks,

        PayBuySellFee,
        PayBaseCommission,

        SellBonds,
        SellStocks,

        EnrollCouponOrDividends,

        WithdrawFromTrustAccount,
        PayWithdrawalTax,
    }

    public static class InvestOperationTypeExt
    {
        public static bool IsBuySell(this InvestOperationType opType)
        {
            return opType == InvestOperationType.BuyBonds || opType == InvestOperationType.SellBonds ||
                opType == InvestOperationType.BuyStocks || opType == InvestOperationType.SellStocks ;
        }

        public static bool IsBond(this InvestOperationType opType)
        {
            return opType == InvestOperationType.BuyBonds || opType == InvestOperationType.SellBonds ;
        }

        public static bool IsStock(this InvestOperationType opType)
        {
            return opType == InvestOperationType.BuyStocks || opType == InvestOperationType.SellStocks ;
        }

    }

   
}