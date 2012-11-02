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
    public Account Article { get; set; }

    public decimal Amount { get; set; }
    public CurrencyCodes Currency { get; set; }
    public string Comment { get; set; }

    public string ToDump()
    {
      return Timestamp + " ; " + Operation + " ; " + 
             Debet.Id + " ; " + Credit.Id + " ; " + Article.Id + " ; " + 
             Amount + " ; " + Currency + " ; " + Comment;
    }

    public Transaction Clone()
    {
      var cloneTransaction = new Transaction();

      cloneTransaction.Timestamp = Timestamp;
      cloneTransaction.Operation = Operation;
      cloneTransaction.Debet = Debet;
      cloneTransaction.Credit = Credit;
      cloneTransaction.Article = Article;
      cloneTransaction.Amount = Amount;
      cloneTransaction.Currency = Currency;
      cloneTransaction.Comment = Comment;

      return cloneTransaction;
    }
  }
}