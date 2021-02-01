using System.IO;
using System.Threading.Tasks;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class DbLoader
    {
        private readonly KeeperDataModel _keeperDataModel;
        private readonly IWindowManager _windowManager;
        private readonly DbLoadingViewModel _dbLoadingViewModel;

        public DbLoader(KeeperDataModel keeperDataModel, IWindowManager windowManager,
            DbLoadingViewModel dbLoadingViewModel)
        {
            _keeperDataModel = keeperDataModel;
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
                    _keeperDataModel.Bin = (KeeperBin)result.Payload;
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