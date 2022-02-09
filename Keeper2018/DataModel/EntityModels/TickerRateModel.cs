using System;
using KeeperDomain;

namespace Keeper2018
{
    public class TickerRateModel
    {
        public int Id { get; set; }
        public TrustTiсker Ticker { get; set; } = new TrustTiсker();
        public DateTime Date { get; set; }
        public double Value { get; set; }
        public int Unit { get; set; } = 1;
        public CurrencyCode Currency { get; set; }

    }
}