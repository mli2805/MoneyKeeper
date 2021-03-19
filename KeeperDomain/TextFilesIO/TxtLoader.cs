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
                LoadCurrencyRates(keeperBin);
                LoadMetalRates(keeperBin);
                LoadAccounts(keeperBin);
                LoadDepoParts(keeperBin);
                LoadTagAssociations(keeperBin);
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

        private static void LoadTagAssociations(KeeperBin keeperBin)
        {
            var content = File.ReadAllLines(PathFactory.GetBackupFilePath("TagAssociations.txt"));
            keeperBin.TagAssociations = content.Select(l => l.TagAssociationFromString()).ToList();
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


        private static void LoadCurrencyRates(KeeperBin bin)
        {
            var content = File.ReadAllLines(PathFactory.GetBackupFilePath("CurrencyRates.txt"));
            bin.Rates = content.Select(s => s.CurrencyRateFromString()).ToList();
        }  
        
        private static void LoadMetalRates(KeeperBin bin)
        {
            var content = File.ReadAllLines(PathFactory.GetBackupFilePath("MetalRates.txt"));
            bin.MetalRates = content.Select(s => s.MetalRateFromString()).ToList();
        }
    }
}