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
                Rates = await NbRbRatesOldTxt.LoadFromOldTxtAsync(),
                AccountPlaneList = AccountsOldTxt.LoadFromOldTxt().ToList()
            };
            keeperBin.TagAssociations = await TagAssociationsOldTxt.LoadFromOldTxtAsync();
            keeperBin.DepositOffers = await BankOffersOldTxt.LoadFromOldTxtAsync();
            keeperBin.Transactions = await TransactionsOldTxt.LoadFromOldTxtAsync();
            return keeperBin;
        }

        //TODO LoadAllFromNewTxt() and SaveAllToNewTxt()
    }
}