using System;
using System.Globalization;

namespace KeeperDomain
{
    public enum Metal
    {
        Gold, Platinum, Silver
    }

    [Serializable]
    public class MinfinMetalRate : IDumpable
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
    }
}
