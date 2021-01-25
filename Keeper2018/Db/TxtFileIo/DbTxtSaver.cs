﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KeeperDomain;

namespace Keeper2018
{
    public static class DbTxtSaver
    {
        public static async Task<bool> SaveAllToNewTxtAsync(this KeeperDb db)
        {
            return await Task.Factory.StartNew(db.SaveAllToNewTxt);
        }

        private static bool SaveAllToNewTxt(this KeeperDb db)
        {
            try
            {
                var currencyRates = db.Bin.Rates.Values.Select(l => l.Dump());
                var accounts = db.Bin.AccountPlaneList.Select(a => a.Dump(db.GetAccountLevel(a))).ToList();
                var deposits = db.Bin.AccountPlaneList.Where(a => a.IsDeposit).Select(m => m.Deposit.Dump());
                var cards = db.Bin.AccountPlaneList.Where(a => a.IsCard).Select(m => m.Deposit.Card.Dump());
                // var depoOffers = db.ExportDepos();
                var newDepoOffers = db.NewExportDepos();
                var transactions = db.Bin.Transactions.Values.OrderBy(t => t.Timestamp).Select(l => l.Dump()).ToList();
                var tagAssociations = db.Bin.TagAssociations.OrderBy(a => a.OperationType).
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

                db.WriteCars();

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Exception during text files saving:{Environment.NewLine} {e.Message}");
                return false;
            }
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

        private static Dictionary<string, List<string>> NewExportDepos(this KeeperDb db)
        {
            var depoRateLines = new List<string>();
            var depoCalcRules = new List<string>();
            var depoConditions = new List<string>();
            var depoOffers = new List<string>();

            foreach (var depositOffer in db.Bin.DepositOffers)
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

        private static void WriteCars(this KeeperDb db)
        {
            var cars = new List<string>();
            var yearMileages = new List<string>();
            foreach (var car in db.Bin.Cars)
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

        public static bool DeleteTxtFiles()
        {
            try
            {
                var backupPath = PathFactory.GetBackupPath();
                if (!Directory.Exists(backupPath)) return false;
                var filenames = Directory.GetFiles(backupPath, "*.txt"); // note: this does not recurse directories! 
                foreach (var filename in filenames)
                    File.Delete(filename);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Exception during database zipping: {e.Message}");
                return false;
            }
        }
    }
}