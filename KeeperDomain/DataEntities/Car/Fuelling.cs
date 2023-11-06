using System;

namespace KeeperDomain
{
    [Serializable]
    public class Fuelling : IDumpable, IParsable<Fuelling>
    {
        public int Id { get; set; } //PK
        public int TransactionId { get; set; }

        public int CarAccountId { get; set; }
        public double Volume { get; set; }
        public FuelType FuelType { get; set; }
        public string Comment { get; set; }

        public string Dump()
        {
            return Id + " ; " + TransactionId + " ; " + CarAccountId + " ; " +
                   Volume + " ; " + FuelType;
        }

        public Fuelling FromString(string s)
        {
            var substrings = s.Split(';');
            Id = int.Parse(substrings[0].Trim());
            TransactionId = int.Parse(substrings[1].Trim());
            CarAccountId = int.Parse(substrings[2].Trim());
            Volume = double.Parse(substrings[3].Trim());
            FuelType = (FuelType)Enum.Parse(typeof(FuelType), substrings[4]);
            return this;
        }
    }
}