using System;

namespace KeeperDomain
{
   
    [Serializable]
    public class NbRbRates : IDumpable, IParsable<NbRbRates>
    {
        public int Id { get; set; } //PK
        public OneRate Usd { get; set; } = new OneRate();
        public OneRate Euro { get; set; } = new OneRate();
        public OneRate Rur { get; set; } = new OneRate();
        public OneRate Cny { get; set; } = new OneRate();

        public double EuroUsdCross => Usd.Value !=0 ? Euro.Value / Usd.Value : 0;

        public string Dump()
        {
            return Usd.Dump() + " | " + Euro.Dump() + " | " + Rur.Dump() + " | " + Cny.Dump();
        }

        public NbRbRates FromString(string s)
        {
            var substrings = s.Split('|');
            Usd = new OneRate().FromString(substrings[0]);
            Euro = new OneRate().FromString(substrings[1]);
            Rur = new OneRate().FromString(substrings[2]);
            Cny = new OneRate().FromString(substrings[3]);
            return this;
        }
    }
}