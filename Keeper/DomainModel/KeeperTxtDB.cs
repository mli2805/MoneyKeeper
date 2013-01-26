using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Data.Entity;

namespace Keeper.DomainModel
{
  //  [Export(typeof(KeeperTxtDb)), PartCreationPolicy(CreationPolicy.Shared)]
  //  public class KeeperTxtDb : DbContext
  //  {
  //    public DbSet<Account> Accounts { get; set; }
  //    public DbSet<Transaction> Transactions { get; set; }
  //    public DbSet<CurrencyRate> CurrencyRates { get; set; }
  //    public DbSet<ArticleAssociation> ArticlesAssociations { get; set; }

  //    protected override void OnModelCreating(DbModelBuilder modelBuilder)
  //    {
  //      modelBuilder.Entity<Account>().Ignore(x => x.IsNotifying);
  //      modelBuilder.Entity<CurrencyRate>().Ignore(x => x.IsNotifying);
  //    }
  //  }

  [Export(typeof(KeeperTxtDb)), PartCreationPolicy(CreationPolicy.Shared)]
  public class KeeperTxtDb
  {
    public ObservableCollection<Account> Accounts { get; set; }
    public ObservableCollection<Transaction> Transactions { get; set; }
    public ObservableCollection<CurrencyRate> CurrencyRates { get; set; }
    public ObservableCollection<ArticleAssociation> ArticlesAssociations { get; set; }

    public List<Account> AccountsPlaneList { get; set; }
  }


}
