using System;
using System.Globalization;

namespace KeeperDomain
{
    [Serializable]
    public class TickerRate
    {
        public int Id { get; set; }
        public int TickerId { get; set; }
        public DateTime Date { get; set; }
        public double Value { get; set; }
        public int Unit { get; set; } = 1;
        public CurrencyCode Currency { get; set; }

        public string Dump()
        {
            return Id + " ; " + TickerId + " ; " + Date.ToString("dd/MM/yyyy") + " ; " +
                   Value.ToString(new CultureInfo("en-US")) + " ; " + Unit + " ; " + Currency;
        }
    }
}