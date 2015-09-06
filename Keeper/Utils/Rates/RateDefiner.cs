using System;
using Keeper.DomainModel;

namespace Keeper.Utils.Rates
{
    class RateDefiner
    {
        public string GetExpression(CurrencyCodes currency1, Decimal amount1, CurrencyCodes currency2, Decimal amount2)
        {
            if (currency1 == currency2) return "ошибка - одинаковая валюта";

            if (currency1 == CurrencyCodes.BYR)
                return RateToString(amount1, amount2);
            if (currency2 == CurrencyCodes.BYR)
                return RateToString(amount2, amount1);

            if (currency1 == CurrencyCodes.RUB)
                return RateToString(amount1, amount2);
            if (currency2 == CurrencyCodes.RUB)
                return RateToString(amount2, amount1);

            if (currency1 == CurrencyCodes.USD)
                return RateToString(amount1, amount2);
            if (currency2 == CurrencyCodes.USD)
                return RateToString(amount2, amount1);

            return "EUR is the most expensive currency in my enum";
        }

        private string RateToString(decimal cheapCurrency, decimal expensiveCurrency)
        {
            return expensiveCurrency != 0 ? String.Format("по курсу {0:#,0}", cheapCurrency / expensiveCurrency) : "";
        }

    }
}
