using System;

namespace KeeperDomain
{
    [Serializable]
    public class CbrRate
    {
        public OneRate Usd { get; set; } = new OneRate();
    }
}