using System.IO;
using System.Threading.Tasks;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class DbLoader
    {
        private readonly KeeperDb _keeperDb;
        private readonly IWindowManager _windowManager;
        private readonly DbLoadingViewModel _dbLoadingViewModel;

        public DbLoader(KeeperDb keeperDb, IWindowManager windowManager,
            DbLoadingViewModel dbLoadingViewModel)
        {
            _keeperDb = keeperDb;
            _windowManager = windowManager;
            _dbLoadingViewModel = dbLoadingViewModel;
        }

        public async Task<bool> Load()
        {
            var path = PathFactory.GetDbFullPath();
            string question;
            if (File.Exists(path))
            {
                var result = await BinSerializer.Deserialize(path);
                if (result.IsSuccess)
                {
                    _keeperDb.Bin = (KeeperBin)result.Payload;
                    return true;
                }
                // question =  $"Ошибка загрузки из файла {path}";
                question = result.Exception.Message;
            }
            else question = $"Файл {path} не найден";
            var vm = new DbAskLoadingViewModel(question);
            _windowManager.ShowDialog(vm);
            if (!vm.Result)
                return false;

            _windowManager.ShowDialog(_dbLoadingViewModel);
            return _dbLoadingViewModel.DbLoaded;
        }

      
    }
}