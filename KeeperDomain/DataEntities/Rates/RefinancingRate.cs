using System;
using System.Globalization;

namespace KeeperDomain
{
    [Serializable]
    public class RefinancingRate : IDumpable
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public double Value { get; set; }

        public string Dump()
        {
            return Id + " ; " + Date.ToString("dd/MM/yyyy") + " ; " + Value.ToString(new CultureInfo("en-US"));
        }
    }
}