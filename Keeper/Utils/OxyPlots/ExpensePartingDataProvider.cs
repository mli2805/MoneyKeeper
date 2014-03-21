using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.Utils.CommonKeeper;

namespace Keeper.Utils.OxyPlots
{
  [Export]
  public class ExpensePartingDataProvider
  {
    private readonly KeeperDb _db;
    private readonly TransactionSetConvertor _transactionSetConvertor;

    [ImportingConstructor]
    public ExpensePartingDataProvider(KeeperDb db, TransactionSetConvertor transactionSetConvertor)
    {
      _db = db;
      _transactionSetConvertor = transactionSetConvertor;
    }

    private IEnumerable<Account> GetExpenseKategories()
    {
      var expense = _db.Accounts.First(a => a.Name == "Все расходы");
      return expense.Children.ToList();
    }

    public List<ExpensePartingDataElement> Get()
    {
      var result = new List<ExpensePartingDataElement>();
      var kategories = GetExpenseKategories();
      var convertedExpenseTransactions = _transactionSetConvertor.ConvertTransactionsQuery(_db.Transactions.Where(t => t.Operation == OperationType.Расход)).ToList();
      foreach (var kategory in kategories)
      {
        var trs = from t in convertedExpenseTransactions where t.Article.Is(kategory) select t;
        var r = from t in trs
                group t by new {t.Timestamp.Month, t.Timestamp.Year}
                into g
                select new ExpensePartingDataElement(kategory, g.Sum(a => a.AmountInUsd), g.Key.Month, g.Key.Year);
        result.AddRange(r);
      }
      return result;
    }
  }

  public class ExpensePartingDataElement
  {
    public Account Kategory { get; set; }
    public decimal Amount { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }

    public ExpensePartingDataElement(Account kategory, decimal amount, int month, int year)
    {
      Kategory = kategory;
      Month = month;
      Amount = amount;
      Year = year;
    }
  }
}
