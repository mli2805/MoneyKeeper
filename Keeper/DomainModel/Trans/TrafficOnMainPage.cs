using System;
using Keeper.DomainModel.Enumes;

namespace Keeper.DomainModel.Trans
{
    public class TrafficOnMainPage
    {
        public DateTime Timestamp;
        public decimal Amount;
        public CurrencyCodes Currency;
        public decimal AmountInUsd;
        public string Comment;

        public override string ToString()
        {
            switch (Currency)
            {
                case CurrencyCodes.USD:
                    return $"{Timestamp:d} {Amount} {Currency.ToString().ToLower()} {Comment}";
                case CurrencyCodes.BYR:
                    return $"{Timestamp:d} {Amount:#,0} {Currency.ToString().ToLower()} ({AmountInUsd:#,0.00}$) {Comment}";
                default:
                    return $"{Timestamp:d} {Amount:#,0.00} {Currency.ToString().ToLower()} ({AmountInUsd:#,0.00}$) {Comment}";
            }

        }
    }
}
