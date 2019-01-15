using System.Collections.ObjectModel;
using System.IO;
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
        private readonly CurrencyRatesViewModel _currencyRatesViewModel;

        public DbLoader(KeeperDb keeperDb, IWindowManager windowManager,
            DbLoadingViewModel dbLoadingViewModel, CurrencyRatesViewModel currencyRatesViewModel)
        {
            _keeperDb = keeperDb;
            _windowManager = windowManager;
            _dbLoadingViewModel = dbLoadingViewModel;
            _currencyRatesViewModel = currencyRatesViewModel;
        }

        public async Task<bool> Load()
        {
            var path = DbIoUtils.GetDbFullPath();
            string question;
            if (File.Exists(path))
            {
                await Task.Delay(1);
                _keeperDb.Bin = DbSerializer.Deserialize(path);
                if (_keeperDb.Bin != null)
                    return true;
                question = $"Ошибка загрузки из файла {path}";
            }
            else question = $"Файл {path} не найден";
            var vm = new DbAskLoadingViewModel(question);
            _windowManager.ShowDialog(vm);
            if (vm.Result == 0)
                return false;

            _dbLoadingViewModel.Initialize(vm.Result);
            _windowManager.ShowDialog(_dbLoadingViewModel);
            return _dbLoadingViewModel.DbLoaded;
        }

        public void ExpandBinToDb(KeeperDb keeperDb)
        {
            _currencyRatesViewModel.Initialize();
            keeperDb.FillInAccountTree(); // must be first

            keeperDb.TagAssociationModels = new ObservableCollection<TagAssociationModel>
                (keeperDb.Bin.TagAssociations.Select(a => a.Map(keeperDb.AcMoDict)));
        }
    }
}