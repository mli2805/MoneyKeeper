using System;

namespace Keeper2018
{
    public class NbRbRateOnScreen
    {
        public DateTime Date { get; set; }
        public CurrenciesValues TodayValues { get; set; }
        public CurrenciesValues YesterdayValues { get; set; }

        public double Basket => BelBaskets.Calculate(TodayValues);
        public double YesterdayBasket => BelBaskets.Calculate(YesterdayValues);
        public string BasketStr => $"{Basket:0.00} ({Basket-YesterdayBasket:0.##})";
    }
}