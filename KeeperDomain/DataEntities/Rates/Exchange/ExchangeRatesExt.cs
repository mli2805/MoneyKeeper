namespace KeeperDomain.Exchange
{
    public static class ExchangeRatesExt
    {
        public static double GetRate(this ExchangeRates line, CurrencyCode fromCurrency, CurrencyCode toCurrency)
        {
            switch (fromCurrency)
            {
                case CurrencyCode.BYN: return line.GetRateFromByn(toCurrency);
                case CurrencyCode.USD: return line.GetRateFromUsd(toCurrency);
                case CurrencyCode.EUR: return line.GetRateFromEur(toCurrency);
                case CurrencyCode.RUB: return line.GetRateFromRub(toCurrency);
            }
            return -1;
        }

        private static double GetRateFromByn(this ExchangeRates line, CurrencyCode toCurrency)
        {
            switch (toCurrency)
            {
                case CurrencyCode.USD: return 1 / line.BynToUsd;
                case CurrencyCode.EUR: return 1 / line.BynToEur;
                case CurrencyCode.RUB: return 1 / line.BynToRub;
            }

            return -1;
        }

        private static double GetRateFromUsd(this ExchangeRates line, CurrencyCode toCurrency)
        {
            switch (toCurrency)
            {
                case CurrencyCode.BYN: return line.UsdToByn;
                case CurrencyCode.EUR: return 1 / line.UsdToEur;
                case CurrencyCode.RUB: return line.UsdToRub;
            }

            return -1;
        }

        private static double GetRateFromEur(this ExchangeRates line, CurrencyCode toCurrency)
        {
            switch (toCurrency)
            {
                case CurrencyCode.BYN: return line.EurToByn;
                case CurrencyCode.USD: return line.EurToUsd;
                case CurrencyCode.RUB: return line.EurToRub;
            }

            return -1;
        }

        private static double GetRateFromRub(this ExchangeRates line, CurrencyCode toCurrency)
        {
            switch (toCurrency)
            {
                case CurrencyCode.BYN: return line.RubToByn;
                case CurrencyCode.USD: return 1 / line.RubToUsd;
                case CurrencyCode.EUR: return 1 / line.RubToEur;
            }

            return -1;
        }
    }
}