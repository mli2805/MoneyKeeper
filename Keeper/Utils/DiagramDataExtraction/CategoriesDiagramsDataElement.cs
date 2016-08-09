using System;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.WorkTypes;

namespace Keeper.Utils.DiagramDataExtraction
{
    public class CategoriesDiagramsDataElement : IComparable<CategoriesDiagramsDataElement>
    {
        public Account Category { get; set; }
        public decimal Amount { get; set; }
        public YearMonth YearMonth { get; set; }

        public CategoriesDiagramsDataElement(Account category, decimal amount, YearMonth yearMonth)
        {
            Category = category;
            Amount = amount;
            YearMonth = yearMonth;
        }

        /// <summary>
        /// ����� ��������� ��������� � ��� ����� ����� ���/��� ��������� ����� ������������ ��� ���������
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(CategoriesDiagramsDataElement other)
        {
            return YearMonth.CompareTo(other.YearMonth);
        }

    }
}