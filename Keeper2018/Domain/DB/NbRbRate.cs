using System;

namespace Keeper2018
{
    public class NbRbRate
    {
        public DateTime Date { get; set; }
        public MainCurrenciesRates Values { get; set; } = new MainCurrenciesRates();

    }
}