using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  public class CurrencyRatesFilter
  {
    public bool IsOn { get; set; }
    public CurrencyCodes Currency { get; set; }

    /// <summary>
    /// ����� ������������� ��������� ����������� ������
    /// ��� �� ����� ������, �� ���������� ��� ������
    /// </summary>
    public CurrencyRatesFilter() { IsOn = false; }

    /// <summary>
    /// � ����� ������ ���������� ������ "����" ������
    /// </summary>
    /// <param name="currency"></param>
    public CurrencyRatesFilter(CurrencyCodes currency)
    {
      IsOn = true;
      Currency = currency;
    }

    public override string ToString()
    {
      return IsOn ? Currency.ToString() : "<no filter>";
    }
  }
}