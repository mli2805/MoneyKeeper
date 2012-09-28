using System;

namespace Keeper.DomainModel
{
  public class Account
  {
    private String _name;
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

    public Account()
    {
      _name = "";
      _balance = 0;
      _parent = null;
    }

    public Account(string name)
    {
      _name = name;
    }
  }
}