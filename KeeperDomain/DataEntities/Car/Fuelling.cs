using System;

namespace KeeperDomain
{
    [Serializable]
    public class Fuelling
    {
        public int Id { get; set; } //PK
        public int TransactionId { get; set; }

        public double Volume { get; set; }
        public FuelType FuelType { get; set; }

        public string Dump()
        {
            return Id + " ; " +
                   TransactionId + " ; " +
                   Volume + " ; " +
                   FuelType;
        }
    }
}