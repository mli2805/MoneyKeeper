using System;
using System.Collections.Generic;

namespace Keeper2018
{
    public static class BelCurrencies
    {
        public static List<BelCurrency> Bys = new List<BelCurrency>()
        {
            new BelCurrency(){ Iso = "BYB", Denomination =     1, From = new DateTime(1992, 7,  1), To = new DateTime(1994,  8, 19), },
            new BelCurrency(){ Iso = "BYB", Denomination =    10, From = new DateTime(1994, 8, 20), To = new DateTime(1999, 12, 31), },
            new BelCurrency(){ Iso = "BYR", Denomination =  1000, From = new DateTime(2000, 1,  1), To = new DateTime(2016,  6, 30), },
            new BelCurrency(){ Iso = "BYN", Denomination = 10000, From = new DateTime(2016, 7,  1), To = DateTime.MaxValue, },
        };

        public static int GetDenominatorForOldTxt(DateTime date)
        {
            if (date < new DateTime(2016, 7, 1))
                return 1;

                return Bys[3].Denomination;

        }
    }
}