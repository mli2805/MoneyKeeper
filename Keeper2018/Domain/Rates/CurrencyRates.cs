using System;

namespace Keeper2018
{
    [Serializable]
    public class CurrencyRates
    {
        public DateTime Date { get; set; }
        public NbRbRates NbRates { get; set; } = new NbRbRates();
        public CbrRate CbrRate { get; set; } = new CbrRate();

        public OneRate MyUsdRate { get; set; } = new OneRate();

    }
}