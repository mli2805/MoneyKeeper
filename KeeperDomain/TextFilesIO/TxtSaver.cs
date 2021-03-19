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
                var currencyRates = bin.Rates.Select(l => l.Dump());
                File.WriteAllLines(PathFactory.GetBackupFilePath("CurrencyRates.txt"), currencyRates);
                if (bin.MetalRates != null)
                {
                    var metalRates = bin.MetalRates.Select(l => l.Dump());
                    File.WriteAllLines(PathFactory.GetBackupFilePath("MetalRates.txt"), metalRates);
                }

                var accounts = bin.DumpWithOffsets();
                File.WriteAllLines(PathFactory.GetBackupFilePath("Accounts.txt"), accounts);
                var deposits = bin.Deposits.OrderBy(d => d.Id).Select(m => m.Dump());
                File.WriteAllLines(PathFactory.GetBackupFilePath("Deposits.txt"), deposits);
                var cards = bin.PayCards.OrderBy(d => d.Id).Select(m => m.Dump());
                File.WriteAllLines(PathFactory.GetBackupFilePath("PayCards.txt"), cards);

                var transactions = bin.Transactions
                    .OrderBy(t => t.Timestamp).
                    Select(l => l.Dump());
                WriteTransactionsContent(PathFactory.GetBackupFilePath("Transactions.txt"), transactions);

                var fuelling = bin.Fuellings.Select(f => f.Dump()).ToList();
                File.WriteAllLines(PathFactory.GetBackupFilePath("Fuellings.txt"), fuelling);

                var tagAssociations = bin.TagAssociations
                    .OrderBy(a => a.OperationType)
                    .ThenBy(b => b.ExternalAccount)
                    .Select(tagAssociation => tagAssociation.Dump());
                File.WriteAllLines(PathFactory.GetBackupFilePath("TagAssociations.txt"), tagAssociations);

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