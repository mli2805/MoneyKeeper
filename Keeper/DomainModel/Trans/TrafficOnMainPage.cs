using System;
using Keeper.DomainModel.Enumes;

namespace Keeper.DomainModel.Trans
{
    public class TrafficOnMainPage
    {
        public DateTime Timestamp;
        public decimal Amount;
        public CurrencyCodes Currency;
        public double Rate;
        public decimal AmountInUsd => Currency == CurrencyCodes.USD ? 
            Amount : 
            Rate.Equals(0) ? 
                -1 : 
                Amount/(decimal) Rate;

        private string AmountInUsdString => AmountInUsd == -1 ? "не задан курс" : $"{AmountInUsd:#,0.00}$";
        public string Comment;

        public override string ToString()
        {
            switch (Currency)
            {
                case CurrencyCodes.USD:
                    return $"{Timestamp:d} {Amount} {Currency.ToString().ToLower()} {Comment}";
                case CurrencyCodes.BYR:
                    return $"{Timestamp:d} {Amount:#,0} {Currency.ToString().ToLower()} ({AmountInUsdString}) {Comment}";
                default:
                    return $"{Timestamp:d} {Amount:#,0.00} {Currency.ToString().ToLower()} ({AmountInUsdString}) {Comment}";
            }

        }
    }
}
