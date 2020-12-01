using System;

namespace KeeperDomain
{
    [Serializable]
    public class YearMileage
    {
        public int Id { get; set; } //PK
        public int CarId { get; set; }
        public int YearNumber { get; set; }
        public int Mileage { get; set; }

        public string Dump()
        {
            return Id + " ; " + CarId + " ; " + YearNumber + " ; " + Mileage;
        }
    }
}