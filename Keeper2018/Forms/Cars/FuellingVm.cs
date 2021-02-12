using System;
using KeeperDomain;

namespace Keeper2018
{
    public class FuellingVm
    {
        public int Id { get; set; } //PK
        public TransactionModel Transaction { get; set; }

        public DateTime Timestamp { get; set; }
        public double Volume { get; set; }
        public FuelType FuelType { get; set; }
        public decimal Amount { get; set; }
        public CurrencyCode Currency { get; set; }
        public string Comment { get; set; }

        public int CarAccountId { get; set; }

        public decimal OneLitrePrice { get; set; }
        public decimal OneLitreInUsd { get; set; }
      
    }
}