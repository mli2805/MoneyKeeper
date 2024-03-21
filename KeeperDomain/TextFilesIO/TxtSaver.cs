﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeeperDomain
{
    public static class TxtSaver
    {
        public static async Task<LibResult> SaveTxtFilesAsync(this KeeperBin bin)
        {
            return await Task.Factory.StartNew(bin.SaveTxtFiles);
        }

        private static LibResult SaveTxtFiles(this KeeperBin bin)
        {
            try
            {
                WriteFileLines(bin.OfficialRates);
                WriteFileLines(bin.ExchangeRates);
                WriteFileLines(bin.MetalRates);
                WriteFileLines(bin.RefinancingRates);

                WriteFileLines(bin.TrustAssets);
                WriteFileLines(bin.TrustAssetRates);
                WriteFileLines(bin.TrustAccounts);
                WriteFileLines(bin.TrustTransactions);

                var accounts = bin.AccountPlaneList.DumpWithOffsets();
                File.WriteAllLines(PathFactory.GetBackupFilePath("Accounts.txt"), accounts);

                WriteFileLines(bin.BankAccounts);
                WriteFileLines(bin.Deposits);
                WriteFileLines(bin.PayCards);
                WriteFileLines(bin.ButtonCollections);

                var transactions = bin.Transactions
                    .OrderBy(t => t.Timestamp)
                    .Select(l => l.Dump());
                WriteTransactionsContent(PathFactory.GetBackupFilePath("Transactions.txt"), transactions);

                WriteFileLines(bin.Fuellings);

                WriteFileLines(bin.DepositOffers);
                WriteFileLines(bin.DepositRateLines);
                WriteFileLines(bin.DepositConditions);

                WriteFileLines(bin.Cars);
                WriteFileLines(bin.CarYearMileages);

                WriteFileLines(bin.CardBalanceMemos, "MemosCardBalance.txt");
                WriteFileLines(bin.SalaryChanges);
                WriteFileLines(bin.LargeExpenseThresholds);

                return new LibResult();
            }
            catch (Exception e)
            {
                return new LibResult(e, "SaveTxtFiles");
            }
        }

        private static void WriteFileLines<T>(List<T> collection, string filename = "") where T : IDumpable
        {
            if (filename == "")
                filename = typeof(T).Name + "s.txt";
            var content = collection.Select(l => l.Dump());
            File.WriteAllLines(PathFactory.GetBackupFilePath(filename), content);
        }

        private static List<string> DumpWithOffsets(this List<Account> accountPlainList)
        {
            var result = new List<string>();
            Stack<int> previousParents = new Stack<int>();
            previousParents.Push(0);
            var previousAccountId = 0;
            var level = 0;
            foreach (var account in accountPlainList)
            {
                while (account.ParentId != previousParents.Peek())
                {
                    if (account.ParentId == previousAccountId)
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

                var dump = account.Dump();
                result.Add(dump.Insert(dump.IndexOf(';') + 1, new string(' ', level * 2)));
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

        public static async Task<LibResult> ZipTxtDbAsync()
        {
            return await Task.Factory.StartNew(Zipper.ZipTxtFiles);
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
                return new LibResult(e, "DeleteTxtFiles");
            }
        }
    }
}