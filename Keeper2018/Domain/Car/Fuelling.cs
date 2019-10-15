using System;

namespace Keeper2018
{
    [Serializable]
    public class Fuelling
    {
        public DateTime Timestamp { get; set; }
        public double Volume { get; set; }
        public FuelType FuelType { get; set; }
        public decimal Amount { get; set; }
        public CurrencyCode Currency { get; set; }
        public string Comment { get; set; }

        public decimal OneLitrePrice { get; set; }
        public decimal OneLitreInUsd { get; set; }
    }
}