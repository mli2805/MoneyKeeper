using System;

namespace KeeperDomain
{
    [Serializable]
    public class StockTiсker
    {
        public int Id { get; set; }

        public string Ticker { get; set; }
        public string Title { get; set; }

        public string Dump()
        {
            return Id + " ; " + Ticker + " ; " + Title;
        }
    }
}