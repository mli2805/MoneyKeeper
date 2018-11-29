using System.Collections.Generic;

namespace Keeper2018
{
    public class BalanceOfAccount
    {
        public readonly Dictionary<CurrencyCode, decimal> Currencies;

        public BalanceOfAccount() { Currencies = new Dictionary<CurrencyCode, decimal>(); }

        public BalanceOfAccount(CurrencyCode currency, decimal amount)
        {
            Currencies = new Dictionary<CurrencyCode, decimal>() { { currency, amount } };
        }
        public void Add(CurrencyCode currency, decimal amount)
        {
            if (Currencies.ContainsKey(currency)) Currencies[currency] = Currencies[currency] + amount; else Currencies.Add(currency, amount);
        }

        public void Sub(CurrencyCode currency, decimal amount)
        {
            if (Currencies.ContainsKey(currency)) Currencies[currency] = Currencies[currency] - amount; else Currencies.Add(currency, -amount);
        }

        public void AddBalance(BalanceOfAccount balanceOfAccount)
        {
            foreach (var currency in balanceOfAccount.Currencies) { Add(currency.Key, currency.Value); }
        }

    }
}