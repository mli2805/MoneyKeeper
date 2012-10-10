using System;

namespace Keeper.DomainModel
{
  public class Transaction
  {
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }

    public OperationType OperationType { get; set; }

    public decimal Amount { get; set; }

    public CurrencyCodes Currency { get; set; }

    public Account Account { get; set; }

    public Account Article { get; set; }

    public string Comment { get; set; }
  }
}