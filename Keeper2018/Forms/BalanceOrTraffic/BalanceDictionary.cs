using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public class BalanceDictionary
    {
        private readonly Dictionary<AccountModel, Balance> _children = new Dictionary<AccountModel, Balance>();

        public void Add(AccountModel child, CurrencyCode currency, decimal amount)
        {
            if (_children.ContainsKey(child)) _children[child].Add(currency, amount);
            else
                _children.Add(child, new Balance(currency, amount));
        }

        public void Sub(AccountModel child, CurrencyCode currency, decimal amount)
        {
            if (_children.ContainsKey(child)) _children[child].Sub(currency, amount);
            else
                _children.Add(child, new Balance(currency, -amount));
        }

        public Balance Summarize()
        {
            var result = new Balance();
            foreach (var child in _children)
            {
                result.AddBalance(child.Value);
            }
            return result;
        }


        public IEnumerable<string> Report()
        {
            foreach (var pair in _children)
            {
                if (pair.Value.Currencies.Any(c => c.Value != 0))
                {
                    yield return "      " + pair.Key.Name;
                    foreach (var currency in pair.Value.Currencies)
                    { if (currency.Value > 0) yield return $"   {currency.Key} {currency.Value:#,#.##}"; }
                }
            }
        }
    }
}