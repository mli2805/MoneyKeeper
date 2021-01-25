using Microsoft.EntityFrameworkCore;

namespace KeeperDomain
{
    // for command line if from package manager console do not work 
    // (check if microsoft.entityframeworkcore.tools nuget package is installed)
    // dotnet ef migrations add --project KeeperDomain --startup-project Keeper2018
    // dotnet ef database update --project KeeperDomain --startup-project Keeper2018
    public class KeeperContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<PayCard> PayCards { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<CurrencyRates> Rates { get; set; }
        public DbSet<TagAssociation> TagAssociations { get; set; }

        public DbSet<DepositRateLine> DepositRateLines { get; set; }
        public DbSet<DepositCalculationRules> DepositCalculationRules { get; set; }
        public DbSet<DepositConditions> DepositConditions { get; set; }
        public DbSet<DepositOffer> DepositOffers { get; set; }

        public DbSet<Car> Cars { get; set; }
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
