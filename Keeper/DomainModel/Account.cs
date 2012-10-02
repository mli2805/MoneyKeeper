using System;

namespace Keeper.DomainModel
{
  public class Account  // соответствует "кошельку"
  {
    private String _name;
    private CurrencyCodes _currency;
    private decimal _balance;
    private Account _parent;

    public string Name
    {
      get { return _name; }
    }

    public decimal Balance
    {
      get { return _balance; }
    }

    public CurrencyCodes Currency { get { return _currency; } }


    public Account()
    {
      _name = "";
      _currency = CurrencyCodes.BYR;
      _balance = 0;
      _parent = null;
    }

    public Account(string name)
    {
      _name = name;
    }
  }
}