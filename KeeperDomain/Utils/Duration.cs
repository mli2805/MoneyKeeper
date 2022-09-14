using System;

namespace KeeperDomain
{
    public enum Durations
    {
        Years, Months, Days, Hours, Minutes, Seconds,
    }

    [Serializable]
    public class Duration
    {
        public bool IsPerpetual { get; set; } // term-less
        public int Value { get; set; }
        public Durations Scale { get; set; }

        public Duration()
        {
            IsPerpetual = true;
        }

        public Duration(int value, Durations scale)
        {
            Value = value;
            Scale = scale;
            IsPerpetual = false;
        }

        public string Dump()
        {
            return IsPerpetual + " ; " + Value + " ; " + Scale;
        }

        public Duration Clone()
        {
            return (Duration)MemberwiseClone();
        }
    }

    

}