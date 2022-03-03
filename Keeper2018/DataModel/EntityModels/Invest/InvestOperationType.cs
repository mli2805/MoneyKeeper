using System.Windows.Media;
using KeeperDomain;

namespace Keeper2018
{
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

        public static Brush FontColor(this InvestOperationType investOperationType)
        {
            switch (investOperationType)
            {
                case InvestOperationType.TopUpTrustAccount: return Brushes.Black;
                case InvestOperationType.BuyBonds: return Brushes.Olive;
                case InvestOperationType.BuyStocks: return Brushes.Olive;
                case InvestOperationType.PayBuySellFee: return Brushes.Red;
                case InvestOperationType.PayBaseCommission: return Brushes.Red;
                case InvestOperationType.SellBonds: return Brushes.Green;
                case InvestOperationType.SellStocks: return Brushes.Green;
                case InvestOperationType.EnrollCouponOrDividends: return Brushes.Blue;
                case InvestOperationType.WithdrawFromTrustAccount: return Brushes.Black;
                case InvestOperationType.PayWithdrawalTax: return Brushes.Red;
                default: return Brushes.Gray;
            }
        }
    }

   
}