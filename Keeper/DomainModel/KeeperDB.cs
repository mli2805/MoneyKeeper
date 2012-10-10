using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keeper.DomainModel
{
  public class KeeperDb : DbContext
  {
    public DbSet<Account> Accounts { get; set; }
    public DbSet<IncomeCategory> Incomes { get; set; }
    public DbSet<ExpenseCategory> Expenses { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Category>()
                  .Map<ExpenseCategory>(m => m.Requires("CatTy").HasValue("e"))
                  .Map<IncomeCategory>(m => m.Requires("CatTy").HasValue("i"));
    }
  }
}
