using System;

namespace Keeper2018
{
   
    [Serializable]
    public class NbRbRates
    {
        public OneRate Usd { get; set; } = new OneRate();
        public OneRate Euro { get; set; } = new OneRate();
        public OneRate Rur { get; set; } = new OneRate();
    }
}