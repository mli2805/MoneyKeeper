using System.Collections.ObjectModel;
using System.Linq;
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
            keeperBin.DepositOffers = await BankOffersOldTxt.LoadFromOldTxtAsync(keeperBin.AccountPlaneList);
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

        public static void ExpandBinToDb(KeeperDb keeperDb)
        {
            keeperDb.FillInAccountTree(); // must be first

            keeperDb.OfficialRates = keeperDb.Bin.OfficialRates; // real expansion will be made in ViewModel c-tor
            keeperDb.AssociationModels = new ObservableCollection<LineModel>
                (keeperDb.Bin.TagAssociations.Select(a=>a.Map(keeperDb.AcMoDict)));
//            keeperDb.DepositOfferModels = new ObservableCollection<DepositOfferModel>
//                (keeperDb.Bin.DepositOffers.Select(x=>x.Map(keeperDb.Bin.AccountPlaneList)));
            keeperDb.TransactionModels = new ObservableCollection<TransactionModel>
                (keeperDb.Bin.Transactions.Select(t=>t.Map(keeperDb.AcMoDict)));
        }

        public static void DbToBin(this KeeperDb db)
        {
            db.FlattenAccountTree();
        }
    }
}