using System;

namespace KeeperDomain
{
    [Serializable]
    public class Fuelling : IDumpable
    {
        public int Id { get; set; } //PK
        public int TransactionId { get; set; }

        public int CarAccountId { get; set; }
        public double Volume { get; set; }
        public FuelType FuelType { get; set; }
        public string Comment { get; set; }

        public string Dump()
        {
            return Id + " ; " +
                   TransactionId + " ; " +
                   CarAccountId + " ; " +
                   Volume + " ; " +
                   FuelType;
        }
    }
}