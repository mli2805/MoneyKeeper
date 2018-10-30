using System;

namespace Keeper2018
{
    public class NbRbRate
    {
        public DateTime Date { get; set; }
        public Rate Usd { get; set; } = new Rate();
        public Rate Euro { get; set; } = new Rate();
        public Rate Rur { get; set; } = new Rate();
    }
}