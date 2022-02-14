using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            _keeperDataModel.Rates = new Dictionary<DateTime, CurrencyRates>();
            foreach (var rate in bin.Rates)
                _keeperDataModel.Rates.Add(rate.Date, rate);
            _keeperDataModel.MetalRates = bin.MetalRates;

            _keeperDataModel.FillInAccountTreeAndDict(bin);
            
            _keeperDataModel.InvestmentAssets = bin.InvestmentAssets;
            _keeperDataModel.AssetRates = bin.AssetRates;
            if (bin.TrustAccounts == null)
                bin.TrustAccounts = new List<TrustAccount>();
            _keeperDataModel.TrustAccounts = bin.TrustAccounts;
            if (bin.InvestmentTransactions == null)
                bin.InvestmentTransactions = new List<InvestmentTransaction>();
            _keeperDataModel.InvestTranModels = 
                bin.InvestmentTransactions.Select(t=>t.Map(_keeperDataModel)).ToList();


            _keeperDataModel.Transactions = new Dictionary<int, TransactionModel>();
            foreach (var transaction in bin.Transactions)
                _keeperDataModel.Transactions.Add(transaction.Id, transaction.Map(_keeperDataModel.AcMoDict));

            _keeperDataModel.FuellingJoinTransaction(bin.Fuellings);

            _keeperDataModel.Cars = bin.JoinCarParts();
            _keeperDataModel.DepositOffers = bin.JoinDepoParts(_keeperDataModel.AcMoDict);
        }

    }
}