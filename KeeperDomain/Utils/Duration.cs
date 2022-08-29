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
        public int Value;
        public Durations Scale;

        public Duration()
        {
        }

        public Duration(int value, Durations scale)
        {
            Value = value;
            Scale = scale;
        }

        public string Dump()
        {
            return Value + " ; " + Scale;
        }
    }

    

}