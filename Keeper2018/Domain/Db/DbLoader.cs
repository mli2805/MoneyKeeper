using System.Collections.ObjectModel;
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
                // AccountTreeViewModel.Accounts = AccountsOldTxt.LoadFromOldTxt();
                keeperDb.Accounts = Accounts2018Txt.LoadFromTxt();
                var rates = await RatesSerializer.DeserializeRates() ?? await NbRbRatesOldTxt.LoadFromOldTxtAsync();
                 
                keeperDb.OfficialRates = new ObservableCollection<OfficialRates>();
                foreach (var rate in rates)
                {
                    keeperDb.OfficialRates.Add(rate);
                }
            }
            return keeperDb;
        }
    }
}