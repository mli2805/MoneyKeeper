using System;
using System.Globalization;

namespace KeeperDomain
{
    [Serializable]
    public class DepositCalculationRules
    {
        public int Id { get; set; } //PK
        public int DepositOfferConditionsId { get; set; }

        public bool IsFactDays { get; set; } // true 28-31/365 false 30/360
        public bool EveryStartDay { get; set; } // ������ ����� ��������
        // �/���
        public bool EveryFirstDayOfMonth { get; set; } // ������ ������ ����� ������
        // �/���
        public bool EveryLastDayOfMonth { get; set; } // ������ ��������� ���� ������
        // � ��� ��������
        public bool IsCapitalized { get; set; }

        public bool IsRateFixed { get; set; }

        public bool HasAdditionalProcent { get; set; }
        public double AdditionalProcent { get; set; }

        public DepositCalculationRules ShallowCopy()
        {
            return (DepositCalculationRules)MemberwiseClone();
        }

        // public string Dump()
        // {
        //     var result = "";
        //
        //     result += IsFactDays ? "1" : "0";
        //     result += EveryStartDay ? "1" : "0";
        //     result += EveryFirstDayOfMonth ? "1" : "0";
        //     result += EveryLastDayOfMonth ? "1" : "0";
        //     result += IsCapitalized ? "1" : "0";
        //     result += IsRateFixed ? "1" : "0";
        //     result += HasAdditionalProcent ? "1" : "0";
        //
        //     result += " + ";
        //     result += AdditionalProcent;
        //
        //     return result;
        // }

        public string Dump()
        {
            return Id  + " ; " + DepositOfferConditionsId  + " ; " + 
                   IsFactDays + " ; " + EveryStartDay + " ; " + EveryFirstDayOfMonth + " ; " + EveryLastDayOfMonth + 
                   " ; " + IsCapitalized + " ; " + IsRateFixed +" ; " + HasAdditionalProcent + " ; " + 
                   AdditionalProcent.ToString(new CultureInfo("en-US"));
        }

    }
}