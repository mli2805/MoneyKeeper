using System;

namespace KeeperDomain
{
   
    [Serializable]
    public class NbRbRates
    {
        public int Id { get; set; } //PK
        public OneRate Usd { get; set; } = new OneRate();
        public OneRate Euro { get; set; } = new OneRate();
        public OneRate Rur { get; set; } = new OneRate();

        public string Dump()
        {
            return Usd.Dump() + " | " + Euro.Dump() + " | " + Rur.Dump();
        }

    }
}