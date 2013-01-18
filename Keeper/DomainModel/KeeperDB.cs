using System.ComponentModel.Composition;
using System.Data.Entity;

namespace Keeper.DomainModel
{
  [Export(typeof(KeeperDb)), PartCreationPolicy(CreationPolicy.Shared)]
  public class KeeperDb : DbContext
  {
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<CurrencyRate> CurrencyRates { get; set; }
    public DbSet<ArticleAssociation> ArticlesAssociations { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Account>().Ignore(x => x.IsNotifying);
      modelBuilder.Entity<CurrencyRate>().Ignore(x => x.IsNotifying);
//      modelBuilder.Entity<Transaction>().Ignore(x => x.IsNotifying);
    }
  }
}
