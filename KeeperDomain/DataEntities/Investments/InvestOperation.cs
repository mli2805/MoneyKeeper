namespace KeeperDomain
{
    public enum InvestOperation
    {
        TopUpTrustAccount,

        BuyBonds,
        BuyStocks,

        PayPurchaseFee,
        PayBaseCommission,

        SellBonds,
        SellStocks,

        EnrollCouponOrDividends,

        WithdrawFromTrustAccount,
    }

    public static class InvestOperationExt
    {
        public static string GetRussian(this InvestOperation investOperation)
        {
            switch (investOperation)
            {
                case InvestOperation.TopUpTrustAccount: return "Пополнить трастовый счет";
                case InvestOperation.BuyBonds: return "Купить облигации";
                case InvestOperation.BuyStocks: return "Купить акции";
                case InvestOperation.PayPurchaseFee: return "Оплатить комиссию за покупку";
                case InvestOperation.PayBaseCommission: return "Оплатить базовую комиссию";
                case InvestOperation.SellBonds: return "Продать облигации";
                case InvestOperation.SellStocks: return "Продать акции";
                case InvestOperation.EnrollCouponOrDividends: return "Зачислить купон или дивиденды";
                case InvestOperation.WithdrawFromTrustAccount: return "Вывести с трастового счета";
                default: return "Неизвестная операция";
            }
        }
    }
}