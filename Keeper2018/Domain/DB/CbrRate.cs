using System;

namespace Keeper2018
{
    [Serializable]
    public class CbrRate
    {
        public OneRate Usd { get; set; } = new OneRate();
    }
}