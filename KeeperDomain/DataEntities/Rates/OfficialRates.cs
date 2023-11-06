using System;
using System.Globalization;

namespace KeeperDomain
{
    [Serializable]
    public class OfficialRates : IDumpable, IParsable<OfficialRates>
    {
        public int Id { get; set; } //PK
        public DateTime Date { get; set; }
        public NbRbRates NbRates { get; set; } = new NbRbRates();
        public CbrRate CbrRate { get; set; } = new CbrRate();

        public string Dump()
        {
            return Id + " ; " + Date.ToString("dd/MM/yyyy") + " ; " +
                   NbRates.Dump() + " ; " + CbrRate.Usd.Dump();
        }

        public OfficialRates FromString(string s)
        {
            var substrings = s.Split(';');
            Id = int.Parse(substrings[0]);
            Date = DateTime.ParseExact(substrings[1].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            NbRates = new NbRbRates().FromString(substrings[2]);
            CbrRate.Usd = new OneRate().FromString(substrings[3]);
            return this;
        }
    }
}