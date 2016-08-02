using System;
using Keeper.DomainModel.Enumes;

namespace Keeper.Utils.Rates
{
    class RateDefiner
    {
        public static string GetExpression(CurrencyCodes currency1, Decimal amount1, CurrencyCodes currency2, Decimal amount2)
        {
            if (currency1 == currency2) return "ошибка - одинаковая валюта";

            if (currency1 == CurrencyCodes.BYR)
                return RateToString(amount1, amount2, true);
            if (currency2 == CurrencyCodes.BYR)
                return RateToString(amount2, amount1, true);

            if (currency1 == CurrencyCodes.BYN)
                return RateToString(amount1, amount2, false);
            if (currency2 == CurrencyCodes.BYN)
                return RateToString(amount2, amount1, false);

            if (currency1 == CurrencyCodes.RUB)
                return RateToString(amount1, amount2, false);
            if (currency2 == CurrencyCodes.RUB)
                return RateToString(amount2, amount1, false);

            if (currency1 == CurrencyCodes.USD)
                return RateToString(amount1, amount2, false);
            if (currency2 == CurrencyCodes.USD)
                return RateToString(amount2, amount1, false);

            return "EUR is the most valuable currency in my enum";
        }

        private static  string RateToString(decimal amountInCheapCurrency, decimal amountInExpensiveCurrency, bool isByr)
        {

            return amountInExpensiveCurrency == 0
                ? ""
                : isByr
                    ? String.Format("по курсу {0:#,0.####}", amountInCheapCurrency/amountInExpensiveCurrency)
                    : String.Format("по курсу {0:#,0.0000}", amountInCheapCurrency/amountInExpensiveCurrency);
        }

    }
}
