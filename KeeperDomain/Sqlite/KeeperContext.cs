using Microsoft.EntityFrameworkCore;

namespace KeeperDomain
{
    // for command line if from package manager console do not work 
    // (check if microsoft.entityframeworkcore.tools nuget package is installed)
    // dotnet ef migrations add --project KeeperDomain --startup-project Keeper2018
    // dotnet ef database update --project KeeperDomain --startup-project Keeper2018
    public class KeeperContext : DbContext
    {
        public DbSet<OfficialRates> Rates { get; set; }
        public DbSet<MetalRate> MetalRates { get; set; }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Deposit> Deposits { get; set; }
        public DbSet<PayCard> PayCards { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<DepositRateLine> DepositRateLines { get; set; }
        public DbSet<DepositConditions> DepositConditions { get; set; }
        public DbSet<DepositOffer> DepositOffers { get; set; }

        public DbSet<Car> Cars { get; set; }
        public DbSet<CarYearMileage> YearMileages { get; set; }
        public DbSet<Fuelling> Fuellings { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var googleDrive = PathFinder.GetGoogleDriveDirectory();
            string dataSourcePath;
            if (string.IsNullOrEmpty(googleDrive))
                dataSourcePath = "Keeper2020.db";
            else
                dataSourcePath = googleDrive + @"\Keeper2018\Sqlite\Keeper2020.db";
            var connectionString = $"Data Source={dataSourcePath}";
            options.UseSqlite(connectionString);
        }
    }
}
