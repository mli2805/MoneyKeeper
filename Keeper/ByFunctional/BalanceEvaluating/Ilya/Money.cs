using Keeper.DomainModel;

namespace Keeper.ByFunctional.BalanceEvaluating.Ilya
{
  public sealed class Money
  {
    public decimal Amount { get; set; }
    public CurrencyCodes Currency { get; set; }
    public Money() { }

    public Money(CurrencyCodes currency, decimal amount)
    {
      Amount = amount;
      Currency = currency;
    }
    public static Money operator -(Money x)
    {
      return new Money(x.Currency, -x.Amount);
    }
    public static MoneyBag operator +(Money x, Money y)
    {
      return new MoneyBag(x) + new MoneyBag(y);
    }
    public static MoneyBag operator -(Money x, Money y)
    {
      if (x.Currency == y.Currency)
        return new MoneyBag(new Money(x.Currency, x.Amount - y.Amount));

      return new MoneyBag(x) - new MoneyBag(y);
    }
  }
}