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
    public DbSet<Category> Categories { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<CurrencyRate> CurrencyRates { get; set; }
  }
}
