using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Keeper2018
{
    public static class DbTxtLoader
    {
        public static async Task<KeeperBin> LoadAllFromNewTxt()
        {
            await Task.Delay(1);
            var keeperBin = new KeeperBin();
            LoadCurrencyRates(keeperBin);
            LoadAccounts(keeperBin);
            LoadDepositOffers(keeperBin);
            LoadTagAssociations(keeperBin);
            LoadTransactions(keeperBin);
            return keeperBin;
        }

        private static void LoadTransactions(KeeperBin keeperBin)
        {
            var content = File.ReadAllLines(DbIoUtils.GetBackupFilePath("Transactions.txt"));
            keeperBin.Transactions = new Dictionary<int, Transaction>();
            for (int i = 0; i < content.Length; i++)
                keeperBin.Transactions.Add(i, content[i].TransactionFromString());
        }

        private static void LoadTagAssociations(KeeperBin keeperBin)
        {
            var content = File.ReadAllLines(DbIoUtils.GetBackupFilePath("TagAssociations.txt"));
            keeperBin.TagAssociations = content.Select(l => l.TagAssociationFromString()).ToList();
        }

        private static void LoadDepositOffers(KeeperBin bin)
        {
            var content = File.ReadAllLines(DbIoUtils.GetBackupFilePath("DepositOffers.txt"));
            bin.DepositOffers = new List<DepositOffer>();
            DepositOffer depositOffer = null;
            foreach (var line in content)
            {
                var parts = line.Split('|');
                if (parts[0] == "::DOFF::")
                {
                    if (depositOffer != null)
                        bin.DepositOffers.Add(depositOffer);
                    depositOffer = parts[1].DepositOfferFromString();
                }
                else if (parts[0] == "::DOES::")
                {
                    depositOffer?.Essentials.Add(Convert.ToDateTime(parts[1], new CultureInfo("ru-RU")),
                        parts[2].DepositEssentialFromString());
                }
                else if (parts[0] == "::DORL::")
                {
                    depositOffer?.Essentials.Last().Value.RateLines.Add(parts[1].DepositRateLineFromString());
                }
            }
        }

        private static void LoadAccounts(KeeperBin bin)
        {
            var content = File.ReadAllLines(DbIoUtils.GetBackupFilePath("Accounts.txt"));
            bin.AccountPlaneList = content.Select(l => l.AccountFromString()).ToList();
            content = File.ReadAllLines(DbIoUtils.GetBackupFilePath("Deposits.txt"));
            foreach (var line in content)
            {
                var deposit = line.DepositFromString();
                bin.AccountPlaneList.First(a => a.Id == deposit.MyAccountId).Deposit = deposit;
            }
        }

        private static void LoadCurrencyRates(KeeperBin bin)
        {
            var content = File.ReadAllLines(DbIoUtils.GetBackupFilePath("CurrencyRates.txt"));
            bin.Rates = new Dictionary<DateTime, CurrencyRates>();
            foreach (var line in content)
            {
                var currencyRate = line.CurrencyRateFromString();
                bin.Rates.Add(currencyRate.Date, currencyRate);
            }
        }
    }
}