using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeeperDomain
{
    public static class TxtSaver
    {
        public static async Task<LibResult> SaveAllToNewTxtAsync(this KeeperBin bin)
        {
            return await Task.Factory.StartNew(bin.SaveAllToNewTxt);
        }

        private static LibResult SaveAllToNewTxt(this KeeperBin bin)
        {
            try
            {
                var currencyRates = bin.OfficialRates.Select(l => l.Dump());
                File.WriteAllLines(PathFactory.GetBackupFilePath("OfficialRates.txt"), currencyRates);
                var exchangeRates = bin.ExchangeRates.Select(l => l.Dump());
                File.WriteAllLines(PathFactory.GetBackupFilePath("ExchangeRates.txt"), exchangeRates);
                var metalRates = bin.MetalRates.Select(l => l.Dump());
                File.WriteAllLines(PathFactory.GetBackupFilePath("MetalRates.txt"), metalRates);
                var refinancingRates = bin.RefinancingRates.Select(l => l.Dump());
                File.WriteAllLines(PathFactory.GetBackupFilePath("RefinancingRates.txt"), refinancingRates);

                var investmentAssets = bin.InvestmentAssets.Select(l => l.Dump());
                File.WriteAllLines(PathFactory.GetBackupFilePath("InvestmentAssets.txt"), investmentAssets);
                var assetRates = bin.AssetRates.Select(l => l.Dump());
                File.WriteAllLines(PathFactory.GetBackupFilePath("AssetRates.txt"), assetRates);

                if (bin.TrustAccounts != null)
                {
                    var trustAccounts = bin.TrustAccounts.Select(l => l.Dump());
                    File.WriteAllLines(PathFactory.GetBackupFilePath("TrustAccounts.txt"), trustAccounts);
                }
                if (bin.InvestmentTransactions != null)
                {
                    var investmentTransactions = bin.InvestmentTransactions.Select(l => l.Dump());
                    File.WriteAllLines(PathFactory.GetBackupFilePath("InvestmentTransactions.txt"), investmentTransactions);
                }

                var accounts = bin.DumpWithOffsets();
                File.WriteAllLines(PathFactory.GetBackupFilePath("Accounts.txt"), accounts);
                var deposits = bin.Deposits.OrderBy(d => d.Id).Select(m => m.Dump());
                File.WriteAllLines(PathFactory.GetBackupFilePath("Deposits.txt"), deposits);
                var cards = bin.PayCards.OrderBy(d => d.Id).Select(m => m.Dump());
                File.WriteAllLines(PathFactory.GetBackupFilePath("PayCards.txt"), cards);
                var buttonCollections = bin.ButtonCollections
                    .OrderBy(c=>c.Id).Select(m => m.Dump());
                File.WriteAllLines(PathFactory.GetBackupFilePath("ButtonCollections.txt"), buttonCollections);

                var transactions = bin.Transactions
                    .OrderBy(t => t.Timestamp).
                    Select(l => l.Dump());
                WriteTransactionsContent(PathFactory.GetBackupFilePath("Transactions.txt"), transactions);

                var fuelling = bin.Fuellings.Select(f => f.Dump()).ToList();
                File.WriteAllLines(PathFactory.GetBackupFilePath("Fuellings.txt"), fuelling);

                var depoOffers = bin.DepositOffers.Select(o => o.Dump());
                File.WriteAllLines(PathFactory.GetBackupFilePath("depoOffers.txt"), depoOffers);
                var depoRateLines = bin.DepositRateLines.Select(o => o.Dump());
                File.WriteAllLines(PathFactory.GetBackupFilePath("depoRateLines.txt"), depoRateLines);
                var depoNewConds = bin.DepoNewConds.Select(o => o.Dump());
                File.WriteAllLines(PathFactory.GetBackupFilePath("depoConds.txt"), depoNewConds);


                var cars = bin.Cars.Select(o => o.Dump());
                File.WriteAllLines(PathFactory.GetBackupFilePath("Cars.txt"), cars);
                var yearMileages = bin.YearMileages.Select(o => o.Dump());
                File.WriteAllLines(PathFactory.GetBackupFilePath("CarYearMileages.txt"), yearMileages);

                return new LibResult();
            }
            catch (Exception e)
            {
                return new LibResult(e);
            }
        }

        private static List<string> DumpWithOffsets(this KeeperBin bin)
        {
            var result = new List<string>();

            Stack<int> previousParents = new Stack<int>();
            previousParents.Push(0);
            var previousAccountId = 0;
            var level = 0;
            foreach (var account in bin.AccountPlaneList)
            {
                if (account.ParentId != previousParents.Peek())
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