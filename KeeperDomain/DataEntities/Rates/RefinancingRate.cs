using System;
using System.Globalization;

namespace KeeperDomain
{
    [Serializable]
    public class RefinancingRate : IDumpable, IParsable<RefinancingRate>
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public double Value { get; set; }

        public string Dump()
        {
            return Id + " ; " + Date.ToString("dd/MM/yyyy") + " ; " + Value.ToString(new CultureInfo("en-US"));
        }

        public RefinancingRate FromString(string s)
        {
            var substrings = s.Split(';');
            Id = int.Parse(substrings[0]);
            Date = DateTime.ParseExact(substrings[1].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            Value = double.Parse(substrings[2], new CultureInfo("en-US"));
            return this;
        }
    }
}