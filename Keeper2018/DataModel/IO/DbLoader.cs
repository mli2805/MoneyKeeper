﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using KeeperDomain;
using KeeperDomain.Exchange;

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
            _keeperDataModel.RememberAll();

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
            _keeperDataModel.OfficialRates = new Dictionary<DateTime, OfficialRates>();
            foreach (var rate in bin.OfficialRates)
                _keeperDataModel.OfficialRates.Add(rate.Date, rate);

            _keeperDataModel.ExchangeRates = new Dictionary<DateTime, ExchangeRates>();
            foreach (var exchangeRate in bin.ExchangeRates)
                _keeperDataModel.ExchangeRates.Add(exchangeRate.Date, exchangeRate);

            _keeperDataModel.MetalRates = bin.MetalRates;
            _keeperDataModel.RefinancingRates = bin.RefinancingRates;

            _keeperDataModel.FillInAccountTreeAndDict(bin);

            _keeperDataModel.AssetRates = bin.TrustAssetRates;
          
            _keeperDataModel.TrustAccounts = bin.TrustAccounts;
            _keeperDataModel.InvestmentAssets = bin.TrustAssets.Select(a => a.Map(_keeperDataModel)).ToList();
            _keeperDataModel.InvestTranModels =
                bin.TrustTransactions.Select(t => t.Map(_keeperDataModel)).ToList();

            _keeperDataModel.Transactions = new Dictionary<int, TransactionModel>();
            foreach (var transaction in bin.Transactions)
                _keeperDataModel.Transactions.Add(transaction.Id, transaction.Map(_keeperDataModel.AcMoDict));

            _keeperDataModel.FuellingJoinTransaction(bin.Fuellings);

            _keeperDataModel.Cars = bin.JoinCarParts();
            _keeperDataModel.DepositOffers = bin.JoinDepoParts(_keeperDataModel.AcMoDict);

            if (bin.CardBalanceMemos == null)
                bin.CardBalanceMemos = new List<CardBalanceMemo>();
            _keeperDataModel.CardBalanceMemoModels = bin.CardBalanceMemos.Select(m=>m.Map(_keeperDataModel.AcMoDict[m.AccountId])).ToList();

            _keeperDataModel.ButtonCollections = bin.ButtonCollections
                .Select(b => b.Map(_keeperDataModel.AcMoDict)).ToList();
        }
    }
}