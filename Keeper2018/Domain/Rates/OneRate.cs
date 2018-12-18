using System;

namespace Keeper2018
{
   
    [Serializable]
    public class OneRate
    {
        public double Value { get; set; }
        public int Unit { get; set; } = 1;

        public OneRate Clone()
        {
            return (OneRate)MemberwiseClone();
        }
    }
}