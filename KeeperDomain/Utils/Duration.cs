using System;

namespace KeeperDomain
{
    public enum Durations
    {
        Years, Months, Days, Hours, Minutes, Seconds,
    }

    [Serializable]
    public class Duration : IDumpable, IParsable<Duration>
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
            if (IsPerpetual)
            {
                return "Perpetual";
            }
            else
            {
                return Value + "-" + Scale;
            }
        }

        public Duration FromString(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return new Duration();
            }

            if (s.Equals("Perpetual", StringComparison.OrdinalIgnoreCase))
            {
                IsPerpetual = true;
                return this;
            }

            var ss = s.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            IsPerpetual = false;
            Value = int.Parse(ss[0]);
            Scale = (Durations)Enum.Parse(typeof(Durations), ss[1]);
            return this;
        }

        public Duration Clone()
        {
            return (Duration)MemberwiseClone();
        }
    }
}