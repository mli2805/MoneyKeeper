using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeeperDomain
{
    public static class DbTxtSaver
    {
        public static async Task<LibResult> SaveAllToNewTxtAsync(this KeeperBin bin)
        {
            return await Task.Factory.StartNew(bin.SaveAllToNewTxt);
        }

        private static LibResult SaveAllToNewTxt(this KeeperBin bin)
        {
            try
            {
                var currencyRates = bin.Rates.Values.Select(l => l.Dump());
                //    var accounts = bin.AccountPlaneList.Select(a => a.Dump(db.GetAccountLevel(a))).ToList();
                var accounts = bin.AccountsExport();
                var deposits = bin.AccountPlaneList.Where(a => a.IsDeposit).Select(m => m.Deposit.Dump());
                var cards = bin.AccountPlaneList.Where(a => a.IsCard).Select(m => m.Deposit.Card.Dump());
                // var depoOffers = db.ExportDepos();
                var newDepoOffers = bin.NewExportDepos();
                var transactions = bin.Transactions.Values.OrderBy(t => t.Timestamp).Select(l => l.Dump()).ToList();
                var tagAssociations = bin.TagAssociations.OrderBy(a => a.OperationType).
                    ThenBy(b => b.ExternalAccount).Select(tagAssociation => tagAssociation.Dump());

                File.WriteAllLines(PathFactory.GetBackupFilePath("CurrencyRates.txt"), currencyRates);
                File.WriteAllLines(PathFactory.GetBackupFilePath("Accounts.txt"), accounts);
                File.WriteAllLines(PathFactory.GetBackupFilePath("Deposits.txt"), deposits);
                File.WriteAllLines(PathFactory.GetBackupFilePath("PayCards.txt"), cards);
                // File.WriteAllLines(PathFactory.GetBackupFilePath("DepositOffers.txt"), depoOffers);
                foreach (var pair in newDepoOffers)
                {
                    File.WriteAllLines(PathFactory.GetBackupFilePath($"{pair.Key}.txt"), pair.Value);
                }
                WriteTransactionsContent(PathFactory.GetBackupFilePath("Transactions.txt"), transactions);
                File.WriteAllLines(PathFactory.GetBackupFilePath("TagAssociations.txt"), tagAssociations);

                bin.WriteCars();

                return new LibResult();
            }
            catch (Exception e)
            {
                return new LibResult(e);
            }
        }

        private static List<string> AccountsExport(this KeeperBin bin)
        {
            var result = new List<string>();

            Stack<int> previousParents = new Stack<int>();
            previousParents.Push(0);
            var previousAccountId = 0;
            var level = 0;
            foreach (var account in bin.AccountPlaneList)
            {
                if (account.OwnerId != previousParents.Peek())
                {
                    if (account.OwnerId == previousAccountId)
                    {
                        level++;
                        previousParents.Push(previousAccountId);
                    }
                    else
                    {
                        level--;
                        previousParents.Pop();
                    }
                }
                result.Add(account.Dump(level));
                previousAccountId = account.Id;
            }
            return result;
        }

        // supposedly it should be faster than File.WriteAllLines because of increased buffer
        private static void WriteTransactionsContent(string filename, IEnumerable<string> content)
        {
            if (File.Exists(filename)) File.Delete(filename);

            const int bufferSize = 65536;  // 64 Kilobytes
            using (var sw = new StreamWriter(filename, true, Encoding.UTF8, bufferSize))
            {
                foreach (var str in content)
                {
                    sw.WriteLine(str);
                }
            }
        }

        // private static List<string> ExportDepos(this KeeperDb db)
        // {
        //     var depoOffers = new List<string>();
        //     foreach (var depositOffer in db.Bin.DepositOffers)
        //     {
        //         depoOffers.Add($"::DOFF::| {depositOffer.Dump()}");
        //         foreach (var pair in depositOffer.ConditionsMap)
        //         {
        //             depoOffers.Add($"::DOES::| {pair.Key:dd/MM/yyyy} | {pair.Value.PartDump()}");
        //             foreach (var depositRateLine in pair.Value.RateLines)
        //             {
        //                 depoOffers.Add($"::DORL::| {depositRateLine.PartDump()}");
        //             }
        //         }
        //     }
        //     return depoOffers;
        // }

        private static Dictionary<string, List<string>> NewExportDepos(this KeeperBin bin)
        {
            var depoRateLines = new List<string>();
            var depoCalcRules = new List<string>();
            var depoConditions = new List<string>();
            var depoOffers = new List<string>();

            foreach (var depositOffer in bin.DepositOffers)
            {
                foreach (var pair in depositOffer.ConditionsMap)
                {
                    depoCalcRules.Add(pair.Value.CalculationRules.Dump());
                    depoRateLines.AddRange(pair.Value.RateLines.Select(rateLine => rateLine.Dump()));
                    depoConditions.Add(pair.Value.Dump());
                }
                depoOffers.Add(depositOffer.Dump());
            }

            return new Dictionary<string, List<string>>
            {
                {"depoOffers", depoOffers},
                {"depoConditions", depoConditions},
                {"depoCalcRules", depoCalcRules},
                {"depoRateLines", depoRateLines}
            };
        }

        private static void WriteCars(this KeeperBin bin)
        {
            var cars = new List<string>();
            var yearMileages = new List<string>();
            foreach (var car in bin.Cars)
            {
                cars.Add(car.Dump());
                yearMileages.AddRange(car.YearMileages.Select(mileage => mileage.Dump()));
            }
            File.WriteAllLines(PathFactory.GetBackupFilePath("Cars.txt"), cars);
            File.WriteAllLines(PathFactory.GetBackupFilePath("CarYearMileages.txt"), yearMileages);
        }

        public static async Task<bool> ZipTxtDbAsync()
        {
            return await Task.Factory.StartNew(Zipper.ZipAllFiles);
        }

        public static LibResult DeleteTxtFiles()
        {
            try
            {
                var backupPath = PathFactory.GetBackupPath();
                if (!Directory.Exists(backupPath)) return new LibResult(new Exception("Backup directory does not exist!"));
                var filenames = Directory.GetFiles(backupPath, "*.txt"); // note: this does not recurse directories! 
                foreach (var filename in filenames)
                    File.Delete(filename);
                return new LibResult();
            }
            catch (Exception e)
            {
                return new LibResult(e);
            }
        }
    }
}