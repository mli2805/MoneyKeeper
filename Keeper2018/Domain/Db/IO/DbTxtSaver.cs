﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ionic.Zip;

namespace Keeper2018
{
    public static class DbTxtSaver
    {
        public static async Task<int> SaveAllToNewTxtAsync(this KeeperDb db)
        {
            return await Task.Factory.StartNew(db.SaveAllToNewTxt);
        }

        private static int SaveAllToNewTxt(this KeeperDb db)
        {
            var currencyRates = db.Bin.Rates.Values.Select(l => l.Dump());
            var accounts = db.Bin.AccountPlaneList.Select(a => a.Dump(db.GetAccountLevel(a))).ToList();
            var deposits = db.Bin.AccountPlaneList.Where(a => a.IsDeposit).Select(m => m.Deposit.Dump());
            var depoOffers = db.ExportDepos();
            var transactions = db.Bin.Transactions.Values.OrderBy(t => t.Timestamp).Select(l => l.Dump()).ToList();
            var tagAssociations = db.Bin.TagAssociations.OrderBy(a => a.OperationType).
                    ThenBy(b => b.ExternalAccount).Select(tagAssociation => tagAssociation.Dump());

            File.WriteAllLines(DbIoUtils.GetBackupFilePath("CurrencyRates.txt"), currencyRates);
            File.WriteAllLines(DbIoUtils.GetBackupFilePath("Accounts.txt"), accounts);
            File.WriteAllLines(DbIoUtils.GetBackupFilePath("Deposits.txt"), deposits);
            File.WriteAllLines(DbIoUtils.GetBackupFilePath("DepositOffers.txt"), depoOffers);
            WriteTransactionsContent(DbIoUtils.GetBackupFilePath("Transactions.txt"), transactions);
            File.WriteAllLines(DbIoUtils.GetBackupFilePath("TagAssociations.txt"), tagAssociations);

            return 0;
        }

        // supposedly it should be faster than File.WriteAllLines because of increased buffer
        private static void WriteTransactionsContent(string filename, IEnumerable<string> content)
        {
            const int bufferSize = 65536;  // 64 Kilobytes
            using (var sw = new StreamWriter(filename, true, Encoding.UTF8, bufferSize))
            {
                foreach (var str in content)
                {
                    sw.WriteLine(str);
                }
            }
        }

        private static List<string> ExportDepos(this KeeperDb db)
        {
            var depoOffers = new List<string>();
            foreach (var depositOffer in db.Bin.DepositOffers)
            {
                depoOffers.Add($"::DOFF::| {depositOffer.Dump()}");
                foreach (var pair in depositOffer.Essentials)
                {
                    depoOffers.Add($"::DOES::| {pair.Key:dd/MM/yyyy} | {pair.Value.PartDump()}");
                    foreach (var depositRateLine in pair.Value.RateLines)
                    {
                        depoOffers.Add($"::DORL::| {depositRateLine.PartDump()}");
                    }
                }
            }
            return depoOffers;
        }

        public static async Task<int> ZipTxtDbAsync()
        {
            await Task.Factory.StartNew(ZipTxtDb);
            return 0;
        }

        public static void ZipTxtDb()
        {
            var archiveName = $"DB{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.zip";
            var zipFileToCreate = Path.Combine(DbIoUtils.GetBackupFilePath(archiveName));
            var backupFolder = DbIoUtils.GetBackupPath();
            try
            {
                using (var zip = new ZipFile())
                {
                    zip.Password = "1";
                    zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                    var filenames =
                        Directory.GetFiles(backupFolder, "*.txt"); // note: this does not recurse directories! 
                    foreach (var filename in filenames)
                        zip.AddFile(filename, string.Empty);
                    zip.Save(zipFileToCreate);
                }
            }
            catch (Exception ex1)
            {
                MessageBox.Show("Exception during database zipping: " + ex1);
            }
        }

        public static void DeleteTxtFiles()
        {
            var backupPath = DbIoUtils.GetBackupPath();
            if (!Directory.Exists(backupPath)) return;
            var filenames = Directory.GetFiles(backupPath, "*.txt"); // note: this does not recurse directories! 
            foreach (var filename in filenames)
                File.Delete(filename);
        }
    }
}