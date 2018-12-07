using System.Linq;
using System.Threading.Tasks;

namespace Keeper2018
{
    public static class DbTexter
    {
        
        public static async Task<KeeperBin> LoadAllFromOldTxt()
        {
            var keeperBin = new KeeperBin
            {
                OfficialRates = await NbRbRatesOldTxt.LoadFromOldTxtAsync(),
                AccountPlaneList = AccountsOldTxt.LoadFromOldTxt().ToList()
            };
            keeperBin.TagAssociations = await TagAssociationsOldTxt.LoadFromOldTxtAsync(keeperBin.AccountPlaneList);
            keeperBin.DepositOffers = await BankOffersOldTxt.LoadFromOldTxtAsync(keeperBin.AccountPlaneList);
            keeperBin.Transactions = await TransactionsOldTxt.LoadFromOldTxtAsync(keeperBin.AccountPlaneList);
            return keeperBin;
        }

        //TODO LoadAllFromNewTxt() and SaveAllToNewTxt()
    }
}