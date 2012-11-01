using System;

namespace Keeper.DomainModel
{
  public class Transaction
  {
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public OperationType Operation { get; set; }

    public Account Debet { get; set; }
    public Account Credit { get; set; }
    public Category Article { get; set; }

    public decimal Amount { get; set; }
    public CurrencyCodes Currency { get; set; }
    public string Comment { get; set; }
  }
}