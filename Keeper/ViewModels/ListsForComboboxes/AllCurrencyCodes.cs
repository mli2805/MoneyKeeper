using System;
using System.Collections.Generic;
using System.Linq;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  /// <summary>
  /// ����������� �����, �.�. ������ ������ ��������� ������������� � ������������ ������
  /// ������ �� ����� ����� ����������� ��������������� � �������� � ������ ���������
  /// </summary>
  public static class AllCurrencyCodes
  {
    public static List<CurrencyCodes> CurrencyList { get; private set; }

    static AllCurrencyCodes()
    {
      CurrencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
    }
  }
}