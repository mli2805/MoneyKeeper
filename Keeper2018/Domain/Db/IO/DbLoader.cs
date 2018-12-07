using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Keeper2018
{
    public class DbLoader
    {
        private readonly KeeperDb _keeperDb;
        private readonly IWindowManager _windowManager;
        private readonly DbLoadingViewModel _dbLoadingViewModel;
        private readonly OfficialRatesViewModel _officialRatesViewModel;

        public DbLoader(KeeperDb keeperDb, IWindowManager windowManager, 
            DbLoadingViewModel dbLoadingViewModel, OfficialRatesViewModel officialRatesViewModel)
        {
            _keeperDb = keeperDb;
            _windowManager = windowManager;
            _dbLoadingViewModel = dbLoadingViewModel;
            _officialRatesViewModel = officialRatesViewModel;
        }

        public async Task<bool> Load()
        {
            _keeperDb.Bin = await DbSerializer.Deserialize();
            if (_keeperDb.Bin == null)
            {
                _windowManager.ShowDialog(_dbLoadingViewModel);
                if (!_dbLoadingViewModel.DbLoaded)
                    return false;
            }
            ExpandBinToDb(_keeperDb);
            return true;
        }

        private void ExpandBinToDb(KeeperDb keeperDb)
        {
            _officialRatesViewModel.Initialize();
            keeperDb.FillInAccountTree(); // must be first

            keeperDb.AssociationModels = new ObservableCollection<LineModel>
                (keeperDb.Bin.TagAssociations.Select(a=>a.Map(keeperDb.AcMoDict)));
//            keeperDb.DepositOfferModels = new ObservableCollection<DepositOfferModel>
//                (keeperDb.Bin.DepositOffers.Select(x=>x.Map(keeperDb.Bin.AccountPlaneList)));
            keeperDb.TransactionModels = new ObservableCollection<TransactionModel>
                (keeperDb.Bin.Transactions.Select(t=>t.Map(keeperDb.AcMoDict)));
        }
    }
}