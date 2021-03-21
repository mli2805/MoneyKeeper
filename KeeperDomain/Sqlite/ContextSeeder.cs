using System;
using System.Threading.Tasks;

namespace KeeperDomain
{
    public static class ContextSeeder
    {
        public static async Task<LibResult> SeedDbContext(this KeeperBin bin)
        {
            try
            {
                using (var dbContext = new KeeperContext())
                {
                    await dbContext.Rates.AddRangeAsync(bin.Rates);

                    await dbContext.Accounts.AddRangeAsync(bin.AccountPlaneList);
                    await dbContext.Deposits.AddRangeAsync(bin.Deposits);
                    await dbContext.PayCards.AddRangeAsync(bin.PayCards);

                    await dbContext.Transactions.AddRangeAsync(bin.Transactions);

                    await dbContext.Cars.AddRangeAsync(bin.Cars);
                    await dbContext.YearMileages.AddRangeAsync(bin.YearMileages);
                    await dbContext.Fuellings.AddRangeAsync(bin.Fuellings);

                    await dbContext.DepositOffers.AddRangeAsync(bin.DepositOffers);
                    await dbContext.DepositConditions.AddRangeAsync(bin.DepoNewConds);
                    await dbContext.DepositRateLines.AddRangeAsync(bin.DepositRateLines);

                    await dbContext.SaveChangesAsync();
                }
                
            }
            catch (Exception e)
            {
                return new LibResult(e);
            }
            return new LibResult();
        }
    }
}
