namespace KeeperDomain
{
    public enum TrustOperation
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

    public static class TrustOperationExt
    {
        public static string GetRussian(this TrustOperation trustOperation)
        {
            switch (trustOperation)
            {
                case TrustOperation.TopUpTrustAccount: return "Пополнить трастовый счет";
                case TrustOperation.BuyBonds: return "Купить облигации";
                case TrustOperation.BuyStocks: return "Купить акции";
                case TrustOperation.PayPurchaseFee: return "Оплатить комиссию за покупку";
                case TrustOperation.PayBaseCommission: return "Оплатить базовую комиссию";
                case TrustOperation.SellBonds: return "Продать облигации";
                case TrustOperation.SellStocks: return "Продать акции";
                case TrustOperation.EnrollCouponOrDividends: return "Зачислить купон или дивиденды";
                case TrustOperation.WithdrawFromTrustAccount: return "Вывести с трастового счета";
                default: return "Неизвестная операция";
            }
        }
    }
}