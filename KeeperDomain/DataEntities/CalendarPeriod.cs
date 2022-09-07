using System;

namespace KeeperDomain
{
    [Serializable]
    public class CalendarPeriod
    {
        public PeriodUnit Unit { get; set; }
        public int Value { get; set; }

        public string Dump()
        {
            return Value + "-" + Unit;
        }

        public static CalendarPeriod Parse(string str)
        {
            var couponPeriod = new CalendarPeriod();
            var ss = str.Split('-');
            if (ss.Length != 2) return couponPeriod;

            if (ss[1] == "years") couponPeriod.Unit = PeriodUnit.years;
            else if (ss[1] == "months") couponPeriod.Unit = PeriodUnit.months;
            else couponPeriod.Unit = PeriodUnit.days;

            couponPeriod.Value = int.Parse(ss[0]);
            return couponPeriod;
        }
    }
}