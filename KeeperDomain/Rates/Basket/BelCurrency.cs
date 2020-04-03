using System;

namespace KeeperDomain.Basket
{
    public class BelCurrency
    {
        public string Iso { get; set; }
        public int Denomination { get; set; }

        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
