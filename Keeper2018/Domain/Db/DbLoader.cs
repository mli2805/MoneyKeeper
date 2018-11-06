using System.Linq;
using System.Threading.Tasks;

namespace Keeper2018
{
    public static class DbLoader
    {
        public static async Task<KeeperDb> Load()
        {
            var keeperDb = await DbSerializer.Deserialize();
            if (keeperDb == null)
            {
                keeperDb = new KeeperDb();

//                keeperDb.Accounts = Accounts2018Txt.LoadFromTxt();
                keeperDb.AccountPlaneList = AccountsOldTxt.LoadFromOldTxt().ToList();

                keeperDb.OfficialRates = await NbRbRatesOldTxt.LoadFromOldTxtAsync();
                keeperDb.Transactions = await TransactionsOldTxt.LoadFromOldTxtAsync(keeperDb.AccountPlaneList);
                keeperDb.DepositOffers = await DepositOffersOldTxt.LoadFromOldTxtAsync(keeperDb.AccountPlaneList);
                keeperDb.TagAssociations = await TagAssociationsOldTxt.LoadFromOldTxtAsync(keeperDb.AccountPlaneList);
            }
            keeperDb.FillInTheTree();
            return keeperDb;
        }
    }
}