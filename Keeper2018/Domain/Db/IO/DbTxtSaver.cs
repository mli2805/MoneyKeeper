using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
            var depoOffers = db.SaveDepoContent();
            var transactions = db.Bin.Transactions.Values.OrderBy(t => t.Timestamp).Select(l => l.Dump());
//            var transactions = db.GetTrans();
            var tagAssociations = db.Bin.TagAssociations.OrderBy(a => a.OperationType).
                    ThenBy(b => b.ExternalAccount).Select(tagAssociation => tagAssociation.Dump());

            File.WriteAllLines(DbIoUtils.GetBackupFilePath("CurrencyRates.txt"), currencyRates);
            File.WriteAllLines(DbIoUtils.GetBackupFilePath("Accounts.txt"), accounts);
            File.WriteAllLines(DbIoUtils.GetBackupFilePath("Deposits.txt"), deposits);
            File.WriteAllLines(DbIoUtils.GetBackupFilePath("DepositOffers.txt"), depoOffers);
            File.WriteAllLines(DbIoUtils.GetBackupFilePath("Transactions.txt"), transactions);
            File.WriteAllLines(DbIoUtils.GetBackupFilePath("TagAssociations.txt"), tagAssociations);

            return 0;
        }

//        private static IEnumerable<string> GetTrans(this KeeperDb db)
//        {
//            int count = -1;
//            foreach (var transaction in db.Bin.Transactions.Values.OrderBy(t => t.Timestamp))
//            {
//                count++;
//                yield return transaction.Dump(count);
//            }
//        }

        private static List<string> SaveDepoContent(this KeeperDb db)
        {
            var depoOffers = new List<string>();
            foreach (var depositOffer in db.Bin.DepositOffers)
            {
                depoOffers.Add($"::DOFF:| {depositOffer.Dump()}");
                foreach (var pair in depositOffer.Essentials)
                {
                    depoOffers.Add($"::DOES::| {pair.Key:dd/MM/yyyy} | {depositOffer.Id} {pair.Value.PartDump()}");
                    foreach (var depositRateLine in pair.Value.RateLines)
                    {
                        depoOffers.Add($"::DORL::| {depositRateLine.PartDump()}");
                    }
                }
            }
            return depoOffers;
        }
    }
}