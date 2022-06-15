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

        public override string ToString()
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

        /// <summary>
        /// if ( CouponPeriod.TryParse("182-days", out CouponPeriod cp) )
        /// </summary>
        /// <param name="str"></param>
        /// <param name="couponPeriod"></param>
        /// <returns></returns>
        public static bool TryParse(string str, out CouponPeriod couponPeriod)
        {
            couponPeriod = new CouponPeriod();
            try
            {
                var ss = str.Split('-');
                if (ss.Length != 2) return false;

                if (ss[1] == "years") couponPeriod.IsYears = true;
                else if (ss[1] == "months") couponPeriod.IsMonths = true;
                else couponPeriod.IsDays = true;

                couponPeriod.Value = int.Parse(ss[0]);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
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