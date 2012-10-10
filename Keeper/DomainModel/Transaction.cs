using System;

namespace Keeper.DomainModel
{
  public class Transaction
  {
    public int Id { get; set; }
//    public DateTime Timestamp { get; set; }
    public OperationType OperationType { get; set; }
    public decimal AmountFrom { get; set; }
    public Account AccountFrom { get; set; }
    public decimal AmountTo { get; set; }
    public Account AccountTo { get; set; }
    public Category Article { get; set; }
    public string Comment { get; set; }
  }
}