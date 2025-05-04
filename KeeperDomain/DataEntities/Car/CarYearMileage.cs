using System;

namespace KeeperDomain
{
    [Serializable]
    public class CarYearMileage : IDumpable, IParsable<CarYearMileage>
    {
        public int Id { get; set; } //PK
        public int CarId { get; set; }
        public int Odometer { get; set; }

        public string Dump()
        {
            return Id + " ; " + CarId + " ; " + Odometer;
        }

        public CarYearMileage FromString(string s)
        {
            var substrings = s.Split(';');

            Id = int.Parse(substrings[0].Trim());
            CarId = int.Parse(substrings[1].Trim());
            Odometer = int.Parse(substrings[2].Trim());

            return this;
        }
    }
}