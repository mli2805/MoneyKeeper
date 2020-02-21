using System;

namespace Keeper2018
{
    public class DepoCurrencyData
    {
        public DateTime StartDate;
        public string Label => $"{StartDate.Month:00}.{StartDate.Year:0000}";
        public decimal DepoRevenue;
        public decimal CurrencyRatesDifferrence;
        public decimal Saldo => DepoRevenue + CurrencyRatesDifferrence;
    }
}
