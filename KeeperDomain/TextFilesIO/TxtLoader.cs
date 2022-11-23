using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KeeperDomain
{
    public static class TxtLoader
    {
        public static async Task<LibResult> LoadAllFromNewTxt()
        {
            await Task.Delay(1);
            try
            {
                var keeperBin = new KeeperBin();
                LoadRates(keeperBin);
                LoadInvestments(keeperBin);
                LoadAccounts(keeperBin);
                LoadDepoParts(keeperBin);
                LoadTransactions(keeperBin);
                LoadCars(keeperBin);
                LoadFuellings(keeperBin);
                return new LibResult(true, keeperBin);
            }
            catch (Exception e)
            {
                return new LibResult(e);
            }
        }

        private static void LoadTransactions(KeeperBin keeperBin)
        {
            var content = File.ReadAllLines(PathFactory.GetBackupFilePath("Transactions.txt"));
            keeperBin.Transactions = content.Select(s => s.TransactionFromString()).ToList();
        }

        private static void LoadDepoParts(KeeperBin bin)
        {
            var content1 = File.ReadAllLines(PathFactory.GetBackupFilePath("depoRateLines.txt"));
            bin.DepositRateLines = content1.Select(l => l.NewDepoRateLineFromString()).ToList();

            var content32 = File.ReadAllLines(PathFactory.GetBackupFilePath("depoConds.txt"));
            bin.DepoNewConds = content32.Select(l => l.DepoNewCondsFromString()).ToList();

            var content4 = File.ReadAllLines(PathFactory.GetBackupFilePath("depoOffers.txt"));
            bin.DepositOffers = content4.Select(l => l.DepositOfferFromString()).ToList();
        }


        private static void LoadCars(KeeperBin bin)
        {
            var carsContent = File.ReadAllLines(PathFactory.GetBackupFilePath("Cars.txt"));
            bin.Cars = carsContent.Select(line => line.CarFromString()).ToList();

            var yearMileageContent = File.ReadAllLines(PathFactory.GetBackupFilePath("CarYearMileages.txt"));
            bin.YearMileages = yearMileageContent.Select(y => y.YearMileageFromString()).ToList();
            var i = 1;
            foreach (var yearMileage in bin.YearMileages)
            {
                yearMileage.Id = i++;
            }
        }


        private static void LoadFuellings(KeeperBin bin)
        {
            var fuellingsContent = File.ReadAllLines(PathFactory.GetBackupFilePath("Fuellings.txt"));
            bin.Fuellings = fuellingsContent.Select(l => l.FuellingFromString()).ToList();
        }

        private static void LoadAccounts(KeeperBin bin)
        {
            var content = File.ReadAllLines(PathFactory.GetBackupFilePath("Accounts.txt"));
            bin.AccountPlaneList = content.Select(l => l.AccountFromString()).ToList();
            var deposits = File.ReadAllLines(PathFactory.GetBackupFilePath("Deposits.txt"));
            bin.Deposits = deposits.Select(d => d.DepositFromString()).ToList();
            var cards = File.ReadAllLines(PathFactory.GetBackupFilePath("PayCards.txt"));
            bin.PayCards = cards.Select(p => p.CardFromString()).ToList();
        }


        private static void LoadRates(KeeperBin bin)
        {
            var content = File.ReadAllLines(PathFactory.GetBackupFilePath("ExchangeRates.txt"));
            bin.ExchangeRates = content.Select(s => s.ExchangeRatesFromString()).ToList();
            content = File.ReadAllLines(PathFactory.GetBackupFilePath("OfficialRates.txt"));
            bin.OfficialRates = content.Select(s => s.CurrencyRateFromString()).ToList();

            content = File.ReadAllLines(PathFactory.GetBackupFilePath("MetalRates.txt"));
            bin.MetalRates = content.Select(s => s.MetalRateFromString()).ToList();

            content = File.ReadAllLines(PathFactory.GetBackupFilePath("RefinancingRates.txt"));
            bin.RefinancingRates = content.Select(s => s.RefinancingRateFromString()).ToList();
        }

        private static void LoadInvestments(KeeperBin bin)
        {
            var assets = File.ReadAllLines(PathFactory.GetBackupFilePath("InvestmentAssets.txt"));
            bin.InvestmentAssets = assets.Select(s => s.InvestmentAssetFromString()).ToList();
            var rates = File.ReadAllLines(PathFactory.GetBackupFilePath("AssetRates.txt"));
            bin.AssetRates = rates.Select(s => s.AssetRateFromString()).ToList();
            var trustAccounts = File.ReadAllLines(PathFactory.GetBackupFilePath("TrustAccounts.txt"));
            bin.TrustAccounts = trustAccounts.Select(s => s.TrustAccountFromString()).ToList();
            var investmentTransactions = File.ReadAllLines(PathFactory.GetBackupFilePath("InvestmentTransactions.txt"));
            bin.InvestmentTransactions = investmentTransactions.Select(s => s.InvestmentTransactionFromString()).ToList();
        }

    }
}