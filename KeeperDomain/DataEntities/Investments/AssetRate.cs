using System;
using System.Globalization;

namespace KeeperDomain
{
    [Serializable]
    public class AssetRate
    {
        public int Id { get; set; }
        public int TickerId { get; set; } = 1;
        public DateTime Date { get; set; } = DateTime.Today;
        public double Value { get; set; }
        public int Unit { get; set; } = 1;
        public CurrencyCode Currency { get; set; } = CurrencyCode.USD;


        public string Dump()
        {
            return Id + " ; " + TickerId + " ; " + Date.ToString("dd/MM/yyyy") + " ; " + Unit + " ; " +
                   Value.ToString(new CultureInfo("en-US")) + " ; " + Currency;
        }
    }
}