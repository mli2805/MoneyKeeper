using System;
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

        public async Task<bool> LoadAndExpand()
        {
            var loadResult = await LoadFromBinOrTxt();
            if (!loadResult.IsSuccess) return false;

            Map((KeeperBin)loadResult.Payload);
            _keeperDataModel.FillInAccountTreeAndDict();
            _keeperDataModel.FuellingJoinTransaction();
            return true;
        }

        private async Task<LibResult> LoadFromBinOrTxt()
        {
            var path = PathFactory.GetDbFullPath();
            LibResult result;
            string question;
            if (File.Exists(path))
            {
                result = await BinSerializer.Deserialize(path);
                if (result.IsSuccess)
                {
                    return result;
                }
                question = result.Exception.Message;
            }
            else
            {
                question = $"Файл {path} не найден";
                result = new LibResult(new Exception(question));
            }
            var vm = new DbAskLoadingViewModel(question);
            _windowManager.ShowDialog(vm);
            if (!vm.Result)
                return result;

            _windowManager.ShowDialog(_dbLoadingViewModel);
            return _dbLoadingViewModel.LoadResult;
        }

        private void Map(KeeperBin bin)
        {
            _keeperDataModel.Rates = bin.Rates;
            _keeperDataModel.AccountPlaneList = bin.AccountPlaneList;
            _keeperDataModel.Transactions = bin.Transactions;
            _keeperDataModel.DepositOffers = bin.DepositOffers;
            _keeperDataModel.Cars = bin.Cars;
            _keeperDataModel.Fuellings = bin.Fuellings;
            _keeperDataModel.TagAssociations = bin.TagAssociations;
        }

    }
}