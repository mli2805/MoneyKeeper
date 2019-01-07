using System;
using System.Globalization;

namespace Keeper2018
{
    [Serializable]
    public class CurrencyRates
    {
        public DateTime Date { get; set; }
        public NbRbRates NbRates { get; set; } = new NbRbRates();
        public CbrRate CbrRate { get; set; } = new CbrRate();

        public OneRate MyUsdRate { get; set; } = new OneRate();

        public string Dump()
        {
            return Convert.ToString(Date.Date, new CultureInfo("ru-RU")) + " ; " +
                   NbRates.Dump() + " ; " + CbrRate.Usd.Dump() + " ; " + MyUsdRate.Dump();
        }
    }
}