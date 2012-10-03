using System;
using System.Collections.Generic;

namespace Keeper.DomainModel
{
  public class Account  // ������������� "��������"
  {
    // �������� (properties) ������
    public string Name { get; set; }
    public CurrencyCodes Currency { get; set; }
    public decimal Balance { get; set; }
    public Account Parent;
    public List<Account> Children { get; set; }

    // ������������
    public Account()
    {
      Name = "";
      Currency = CurrencyCodes.BYR;
      Balance = 0;
      Parent = null;
      Children = new List<Account>();
    }

    public Account(string name)
      :this()  // �.�. ������� ����������� ��� ����������, � ����� ��������� ���� ���
    {
      Name = name;
    }
  }
}