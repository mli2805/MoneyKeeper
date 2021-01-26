using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KeeperDomain
{
    public static class DbTxtLoader
    {
        public static async Task<LibResult> LoadAllFromNewTxt()
        {
            await Task.Delay(1);
            try
            {
                var keeperBin = new KeeperBin();
                LoadCurrencyRates(keeperBin);
                LoadAccounts(keeperBin);
                // LoadDepositOffers(keeperBin);
                NewLoadDepositOffers(keeperBin);
                LoadTagAssociations(keeperBin);
                LoadTransactions(keeperBin);
                LoadCars(keeperBin);
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
            keeperBin.Transactions = new Dictionary<int, Transaction>();
            for (int i = 0; i < content.Length; i++)
                keeperBin.Transactions.Add(i, content[i].TransactionFromString());
        }

        private static void LoadTagAssociations(KeeperBin keeperBin)
        {
            var content = File.ReadAllLines(PathFactory.GetBackupFilePath("TagAssociations.txt"));
            keeperBin.TagAssociations = content.Select(l => l.TagAssociationFromString()).ToList();
        }

        // private static void LoadDepositOffers(KeeperBin bin)
        // {
        //     var content = File.ReadAllLines(PathFactory.GetBackupFilePath("DepositOffers.txt"));
        //     bin.DepositOffers = new List<DepositOffer>();
        //     DepositOffer depositOffer = null;
        //     foreach (var line in content)
        //     {
        //         var parts = line.Split('|');
        //         if (parts[0] == "::DOFF::")
        //         {
        //             if (depositOffer != null)
        //                 bin.DepositOffers.Add(depositOffer);
        //             depositOffer = parts[1].DepositOfferFromString();
        //         }
        //         else if (parts[0] == "::DOES::")
        //         {
        //             depositOffer?.ConditionsMap.Add(DateTime.ParseExact(parts[1].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture),
        //                 parts[2].DepositEssentialFromString());
        //         }
        //         else if (parts[0] == "::DORL::")
        //         {
        //             depositOffer?.ConditionsMap.Last().Value.RateLines.Add(parts[1].DepositRateLineFromString());
        //         }
        //     }
        //     bin.DepositOffers.Add(depositOffer);
        // }

        private static void NewLoadDepositOffers(KeeperBin bin)
        {
            var content1 = File.ReadAllLines(PathFactory.GetBackupFilePath("depoRateLines.txt"));
            var depoRateLines = content1.Select(l => l.NewDepoRateLineFromString()).ToList();

            var content2 = File.ReadAllLines(PathFactory.GetBackupFilePath("depoCalcRules.txt"));
            var depoCalcRules = content2.Select(l => l.NewDepoCalcRulesFromString()).ToList();

            var content3 = File.ReadAllLines(PathFactory.GetBackupFilePath("depoConditions.txt"));
            var depoConditions = content3.Select(l => l.DepoConditionsFromString()).ToList();
            foreach (var depoCondition in depoConditions)
            {
                depoCondition.CalculationRules =
                    depoCalcRules.First(cr => cr.DepositOfferConditionsId == depoCondition.Id);

                depoCondition.RateLines = depoRateLines
                    .Where(l => l.DepositOfferConditionsId == depoCondition.Id)
                    .OrderBy(r => r.DateFrom)
                    .ThenBy(r => r.AmountFrom).ToList();
            }

            var content4 = File.ReadAllLines(PathFactory.GetBackupFilePath("depoOffers.txt"));
            bin.DepositOffers = content4.Select(l => l.DepositOfferFromString()).ToList();
            foreach (var depoOffer in bin.DepositOffers)
            {
                foreach (var condition in depoConditions.Where(c => c.DepositOfferId == depoOffer.Id))
                {
                    depoOffer.ConditionsMap.Add(condition.DateFrom, condition);   
                }
            }
        }

        private static void LoadCars(KeeperBin bin)
        {
            var carsContent = File.ReadAllLines(PathFactory.GetBackupFilePath("Cars.txt"));
            var yearMileageContent = File.ReadAllLines(PathFactory.GetBackupFilePath("CarYearMileages.txt"));
            bin.Cars = carsContent.Select(line => line.CarFromString()).ToList();
            var yearMileages = yearMileageContent.Select(line => line.YearMileageFromString()).ToList();
            foreach (var car in bin.Cars)
                car.YearMileages = yearMileages.Where(l => l.CarId == car.CarAccountId).ToArray();
        }

        private static void LoadAccounts(KeeperBin bin)
        {
            var content = File.ReadAllLines(PathFactory.GetBackupFilePath("Accounts.txt"));
            bin.AccountPlaneList = content.Select(l => l.AccountFromString()).ToList();
            var deposits = File.ReadAllLines(PathFactory.GetBackupFilePath("Deposits.txt"));
            foreach (var depo in deposits)
            {
                try
                {
                    var deposit = depo.DepositFromString();
                    bin.AccountPlaneList.First(a => a.Id == deposit.MyAccountId).Deposit = deposit;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }
            var cards = File.ReadAllLines(PathFactory.GetBackupFilePath("PayCards.txt"));
            foreach (var line in cards)
            {
                var card = line.CardFromString();
                bin.AccountPlaneList.First(a => a.Id == card.MyAccountId).Deposit.Card = card;
            }
        }

        private static void LoadCurrencyRates(KeeperBin bin)
        {
            var content = File.ReadAllLines(PathFactory.GetBackupFilePath("CurrencyRates.txt"));
            bin.Rates = new Dictionary<DateTime, CurrencyRates>();
            foreach (var line in content)
            {
                var currencyRate = line.CurrencyRateFromString();
                bin.Rates.Add(currencyRate.Date, currencyRate);
            }
        }
    }
}