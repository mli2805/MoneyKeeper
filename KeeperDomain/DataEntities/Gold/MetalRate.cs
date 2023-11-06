using System;
using System.Globalization;

namespace KeeperDomain
{
    public enum Metal
    {
        Gold, Platinum, Silver
    }

    [Serializable]
    public class MetalRate : IDumpable, IParsable<MetalRate>
    {
        public int Id { get; set; } //PK
        public DateTime Date { get; set; }
        public Metal Metal { get; set; }
        public int Proba { get; set; }
        public double Price { get; set; }

        
        public string Dump()
        {
            return Id + " ; " + Date.ToString("dd/MM/yyyy") + " ; " +
                   Metal + " ; " + Proba + " ; " + Price.ToString(new CultureInfo("en-US"));
        }

        public MetalRate FromString(string s)
        {
            var substrings = s.Split(';');
            Id = int.Parse(substrings[0]);
            Date = DateTime.ParseExact(substrings[1].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            Metal = (Metal)Enum.Parse(typeof(Metal), substrings[2]);
            Proba = int.Parse(substrings[3]);
            Price = double.Parse(substrings[4], new CultureInfo("en-US"));
            return this;
        }
    }
}
