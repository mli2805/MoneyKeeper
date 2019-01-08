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
            var transactions = db.Bin.Transactions.Values.OrderBy(t => t.Timestamp).Select(l => l.Dump());
            var tagAssociations = db.Bin.TagAssociations.OrderBy(a => a.OperationType).
                    ThenBy(b => b.ExternalAccount).Select(tagAssociation => tagAssociation.Dump());

            File.WriteAllLines(DbIoUtils.GetBackupFilePath("CurrencyRates"), currencyRates);
            File.WriteAllLines(DbIoUtils.GetBackupFilePath("Accounts"), accounts);
            File.WriteAllLines(DbIoUtils.GetBackupFilePath("Deposits"), deposits);
            File.WriteAllLines(DbIoUtils.GetBackupFilePath("Transactions"), transactions);
            File.WriteAllLines(DbIoUtils.GetBackupFilePath("TagAssociations"), tagAssociations);

            db.SaveDepoContent2();
            return 0;
        }

        private static void SaveDepoContent1(this KeeperDb db)
        {
            var depoOffers = new List<string>();
            var depoEssentials = new List<string>();
            var depoRateLines = new List<string>();
            foreach (var depositOffer in db.Bin.DepositOffers)
            {
                depoOffers.Add(depositOffer.Dump());
                foreach (var pair in depositOffer.Essentials)
                {
                    depoEssentials.Add($"{pair.Key:dd/MM/yyyy} {depositOffer.Id} {pair.Value.PartDump()}");
                    foreach (var depositRateLine in pair.Value.RateLines)
                    {
                        depoRateLines.Add(depositRateLine.PartDump());
                    }
                }
            }

            File.WriteAllLines(DbIoUtils.GetBackupFilePath("DepositOffers"), depoOffers);
            File.WriteAllLines(DbIoUtils.GetBackupFilePath("DepositEssentials"), depoEssentials);
            File.WriteAllLines(DbIoUtils.GetBackupFilePath("DepositRateLines"), depoRateLines);
        } 
        
        private static void SaveDepoContent2(this KeeperDb db)
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

            File.WriteAllLines(DbIoUtils.GetBackupFilePath("DepositOffers"), depoOffers);
        }
    }
}