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
        public static string GetRussian(this InvestOperationType investOperationType)
        {
            switch (investOperationType)
            {
                case InvestOperationType.TopUpTrustAccount: return "Пополнить трастовый счет";
                case InvestOperationType.BuyBonds: return "Купить облигации";
                case InvestOperationType.BuyStocks: return "Купить акции";
                case InvestOperationType.PayBuySellFee: return "Оплатить комиссию за операцию";
                case InvestOperationType.PayBaseCommission: return "Оплатить базовую комиссию";
                case InvestOperationType.SellBonds: return "Продать облигации";
                case InvestOperationType.SellStocks: return "Продать акции";
                case InvestOperationType.EnrollCouponOrDividends: return "Зачислить купон или дивиденды";
                case InvestOperationType.WithdrawFromTrustAccount: return "Вывести с трастового счета";
                case InvestOperationType.PayWithdrawalTax: return "Оплатить налог на вывод средств";
                default: return "Неизвестная операция";
            }
        }
    }

   
}