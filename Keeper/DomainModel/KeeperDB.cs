using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keeper.ViewModels;

namespace Keeper.DomainModel
{
  [Export(typeof(KeeperDb)), PartCreationPolicy(CreationPolicy.Shared)]
  public class KeeperDb : DbContext
  {
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<CurrencyRate> CurrencyRates { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Account>().Ignore(x => x.IsNotifying);
      modelBuilder.Entity<CurrencyRate>().Ignore(x => x.IsNotifying);
//      modelBuilder.Entity<Transaction>().Ignore(x => x.IsNotifying);
    }
  }
}
