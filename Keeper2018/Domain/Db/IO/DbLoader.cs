﻿using System.Linq;
using System.Threading.Tasks;

namespace Keeper2018
{
    public static class DbLoader
    {
        public static async Task<KeeperBin> LoadAllFromOldTxt()
        {
            var keeperBin = new KeeperBin
            {
                AccountPlaneList = AccountsOldTxt.LoadFromOldTxt().ToList()
            };
            keeperBin.TagAssociations = await TagAssociationsOldTxt.LoadFromOldTxtAsync(keeperBin.AccountPlaneList);
            keeperBin.DepositOffers = await DepositOffersOldTxt.LoadFromOldTxtAsync(keeperBin.AccountPlaneList);
            keeperBin.OfficialRates = await NbRbRatesOldTxt.LoadFromOldTxtAsync();
            keeperBin.Transactions = await TransactionsOldTxt.LoadFromOldTxtAsync(keeperBin.AccountPlaneList);
            return keeperBin;
        }

//        private static async Task<KeeperBin> LoadFromTxt()
//        {
//            await Task.Delay(1);
//            var keeperBin = new KeeperBin
//            {
//                AccountPlaneList = Accounts2018Txt.LoadFromTxt().ToList()
//            };
//            return keeperBin;
//        }

        public static async Task<int> ExpandBinToDb(KeeperDb keeperDb)
        {
            await Task.Delay(1);
            keeperDb.FillInTheTree(); // must be first

            keeperDb.OfficialRates = keeperDb.Bin.OfficialRates; // real expansion will be made in ViewModel c-tor
            keeperDb.AssociationsToModels();
            keeperDb.TransToModels();
            return 1;
        }
    }
}