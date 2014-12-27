using System;

namespace Keeper.DomainModel.Deposit
{
    [Serializable]
    public class BankDepositCalculatingRules
    {
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
        public decimal AdditionalProcent { get; set; }
    }
}