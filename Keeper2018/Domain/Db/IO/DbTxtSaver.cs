using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Keeper2018
{
    public static class DbTxtSaver
    {
        public static async Task<int> SaveAllToNewTxtAsync(this KeeperDb db)
        {
            var result = await Task.Factory.StartNew(db.SaveAllToNewTxt);
            Console.WriteLine($@"{result}");
            return result;
        }

        public static int SaveAllToNewTxt(this KeeperDb db)
        {
            var currencyRates = db.Bin.Rates.Values.Select(l => l.Dump());
            var accounts = db.Bin.AccountPlaneList.Select(a => a.Dump(db.GetAccountLevel(a))).ToList();
            var deposits = db.Bin.AccountPlaneList.Where(a => a.IsDeposit).Select(m => m.Deposit.Dump());
            var depositOffers = db.Bin.DepositOffers.Select(o => o.Dump());
            var transactions = db.Bin.Transactions.Values.OrderBy(t => t.Timestamp).Select(l => l.Dump());
            var tagAssociations = db.Bin.TagAssociations.OrderBy(a => a.OperationType).
                    ThenBy(b => b.ExternalAccount).Select(tagAssociation => tagAssociation.Dump());

            File.WriteAllLines(DbIoUtils.GetBackupFilePath("CurrencyRates"), currencyRates);
            File.WriteAllLines(DbIoUtils.GetBackupFilePath("Accounts"), accounts);
            File.WriteAllLines(DbIoUtils.GetBackupFilePath("Deposits"), deposits);
            File.WriteAllLines(DbIoUtils.GetBackupFilePath("DepositOffers"), depositOffers);
            File.WriteAllLines(DbIoUtils.GetBackupFilePath("Transactions"), transactions);
            File.WriteAllLines(DbIoUtils.GetBackupFilePath("TagAssociations"), tagAssociations);
            return 234;
        }

    }
}