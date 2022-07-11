using System;

namespace KeeperDomain
{
    [Serializable]
    public class CouponPeriod
    {
        public bool IsYears { get; set; }
        public bool IsMonths { get; set; }
        public bool IsDays { get; set; }
        public int Value { get; set; }

        public string Dump()
        {
            string word;
            if (IsYears)
                word = "years";
            else if (IsMonths)
                word = "months";
            else
                word = "days";

            return $"{Value}-{word}";
        }

        public override string ToString()
        {
            if (Value == 0) return "";

            string word;
            if (IsYears)
                word = Value.YearsNumber();
            else if (IsMonths)
                word = Value.MonthsNumber();
            else
                word = Value.DaysNumber();

            return $"{Value} {word}";
        }

        public static CouponPeriod Parse(string str)
        {
            var couponPeriod = new CouponPeriod();
            var ss = str.Split('-');
            if (ss.Length != 2) return couponPeriod;

            if (ss[1] == "years") couponPeriod.IsYears = true;
            else if (ss[1] == "months") couponPeriod.IsMonths = true;
            else couponPeriod.IsDays = true;

            couponPeriod.Value = int.Parse(ss[0]);
            return couponPeriod;
        }
    }
}