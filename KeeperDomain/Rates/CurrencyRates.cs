using System;

namespace KeeperDomain
{
    [Serializable]
    public class CurrencyRates
    {
        public DateTime Date { get; set; }
        public NbRbRates NbRates { get; set; } = new NbRbRates();
        public CbrRate CbrRate { get; set; } = new CbrRate();

        public OneRate MyEurUsdRate { get; set; } = new OneRate();
        public OneRate MyUsdRate { get; set; } = new OneRate();

        public string Dump()
        {
            return Date.ToString("dd/MM/yyyy") + " ; " +
                   NbRates.Dump() + " ; " + CbrRate.Usd.Dump() + " ; " + MyUsdRate.Dump() + " ; " + MyEurUsdRate.Dump();
        }
    }
}