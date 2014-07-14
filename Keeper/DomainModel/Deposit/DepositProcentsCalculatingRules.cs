using System;

namespace Keeper.DomainModel
{
    [Serializable]
    public class DepositProcentsCalculatingRules
    {
        public bool IsFactDays { get; set; } // true 28-31/365 false 30/360

        public bool OnlyAtTheEnd { get; set; } // ��������� �������� ��������� �������� ������ � ����� �����
        // �����
        public bool EveryStartDay { get; set; } // ������ ����� ��������
        // �/���
        public bool EveryFirstDayOfMonth { get; set; } // ������ ������ ����� ������
        // �/���
        public bool EveryLastDayOfMonth { get; set; } // ������ ��������� ���� ������
        // � ��� ��������
        public bool IsCapitalized { get; set; }
    }
}