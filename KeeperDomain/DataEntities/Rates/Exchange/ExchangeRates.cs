﻿using System;
using System.Globalization;

namespace KeeperDomain.Exchange
{
    /// <summary>
    /// 
    /// Let's rates are 2.5 - 2.6
    /// I have 250 byn, I can buy 250 * (1 / 2.6) = 96 usd
    /// I have 100 usd, I can buy 100 * 2.5 = 250 byn
    /// 
    /// I have 56000 rub, I can buy 56000/72.5 = 772 usd ==  56000* (1/72.5) 
    /// I have 800 usd, I can buy 800*56 = 44800 rub
    /// 
    ///  /// </summary>
    [Serializable]
    public class ExchangeRates
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }


        // на табло 2.5 - 2.6
        public double UsdToByn { get; set; } // 2.5
        public double BynToUsd { get; set; } // 2.6

        public string BynUsd => $"{UsdToByn} - {BynToUsd}";

        public double EurToByn { get; set; }
        public double BynToEur { get; set; }

        public string BynEur => $"{EurToByn} - {BynToEur}";

        // хранить не за 100, а за 1 (для унификации)
        public double RubToByn { get; set; }
        public double BynToRub { get; set; }

        public string BynRub => $"{RubToByn} - {BynToRub}";
      
        // 1.05 - 1.10
        public double EurToUsd { get; set; } // 1.05
        public double UsdToEur { get; set; } // 1.10

        public string UsdEur => $"{EurToUsd} - {UsdToEur}";
      
        // 56 - 72.5
        public double UsdToRub { get; set; } // 56
        public double RubToUsd { get; set; } // 72.5

        public string UsdRub => $"{UsdToRub} - {RubToUsd}";
        
        public double EurToRub { get; set; }
        public double RubToEur { get; set; }

        public string EurRub => $"{EurToRub} - {RubToUsd}";
        
        public string Dump()
        {
            return Id + " ; " + Date.ToString("dd/MM/yyyy") + " ; " +
                   UsdToByn.ToString(new CultureInfo("en-Us")) + " ; " +
                   BynToUsd.ToString(new CultureInfo("en-Us")) + " ; " +
                   EurToByn.ToString(new CultureInfo("en-Us")) + " ; " +
                   BynToEur.ToString(new CultureInfo("en-Us")) + " ; " +
                   RubToByn.ToString(new CultureInfo("en-Us")) + " ; " +
                   BynToRub.ToString(new CultureInfo("en-Us")) + " ; " +
                   EurToUsd.ToString(new CultureInfo("en-Us")) + " ; " +
                   UsdToEur.ToString(new CultureInfo("en-Us")) + " ; " +
                   UsdToRub.ToString(new CultureInfo("en-Us")) + " ; " +
                   RubToUsd.ToString(new CultureInfo("en-Us")) + " ; " +
                   EurToRub.ToString(new CultureInfo("en-Us")) + " ; " +
                   RubToEur.ToString(new CultureInfo("en-Us"));
        }

        public ExchangeRates Clone()
        {
            return (ExchangeRates)MemberwiseClone();
        }

      
    }
}