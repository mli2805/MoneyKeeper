using Microsoft.EntityFrameworkCore;

namespace KeeperDomain
{
    // dotnet ef database update --project KeeperSqliteDb --startup-project Keeper2018
    public class KeeperContext: DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<PayCard> PayCards { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<CurrencyRates> Rates { get; set; }
        public DbSet<TagAssociation> TagAssociations { get; set; }


        public DbSet<DepositRateLine> DepositRateLines { get; set; }
        public DbSet<DepositConditions> DepositConditions { get; set; }
     //   public DbSet<DepositOffer> DepositOffer { get; set; }

        public DbSet<Car> Cars { get; set; }
        public DbSet<Fuelling> Fuellings { get; set; }
        public DbSet<YearMileage> YearMileages { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var googleDrive = PathFinder.GetGoogleDriveDirectory();
            string dataSourcePath;
            if (string.IsNullOrEmpty(googleDrive))
                dataSourcePath = "Keeper2020.db";
            else
                dataSourcePath = googleDrive + @"\Keeper2018\sqlite\Keeper2020.db";
            var connectionString = $"Data Source={dataSourcePath}";
            options.UseSqlite(connectionString);
        }
    }
}
