using System;
using System.Globalization;

namespace KeeperDomain
{
   
    [Serializable]
    public class OneRate
    {
        public int Id { get; set; }
        public double Value { get; set; }
        public int Unit { get; set; } = 1;

        public OneRate Clone()
        {
            return (OneRate)MemberwiseClone();
        }

        public string Dump()
        {
            return Value.ToString(new CultureInfo("en-US")) + " / " + Unit;
        }
    }
}