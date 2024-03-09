using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AutoMapper;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class DbSaver
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingModelsToEntitiesProfile>()).CreateMapper();

        private readonly KeeperDataModel _keeperDataModel;
        private readonly IWindowManager _windowManager;
        private readonly ShellPartsBinder _shellPartsBinder;
        private readonly TransactionsViewModel _transactionsViewModel;

        public DbSaver(KeeperDataModel keeperDataModel, IWindowManager windowManager,
            ShellPartsBinder shellPartsBinder, TransactionsViewModel transactionsViewModel)
        {
            _keeperDataModel = keeperDataModel;
            _windowManager = windowManager;
            _shellPartsBinder = shellPartsBinder;
            _transactionsViewModel = transactionsViewModel;
        }

        public async Task<bool> SaveWithUi()
        {
            if (_shellPartsBinder.IsBusy) return false;
            _shellPartsBinder.IsBusy = true;
            _shellPartsBinder.FooterVisibility = Visibility.Visible;

            var result = await SaveData();
            if (!result.IsSuccess)
            {
                var lines = new List<string>()
                    {
                        "Exception during DATA saving:",
                        result.Where,
                        result.Exception.Message
                    };
                var vm = new MyMessageBoxViewModel(MessageType.Error, lines);
                _windowManager.ShowDialog(vm);
            }

            _shellPartsBinder.FooterVisibility = Visibility.Collapsed;
            _shellPartsBinder.IsBusy = false;

            return result.IsSuccess;
        }

        private async Task<LibResult> SaveData()
        {
            var mappingResult = await MapBack();
            if (!mappingResult.IsSuccess)
                return mappingResult;

            var bin = (KeeperBin)mappingResult.Payload;

            var binSerializerResult = await BinSerializer.Serialize(bin);
            if (!binSerializerResult.IsSuccess)
                return binSerializerResult;

            var saveTxtResult = await bin.SaveTxtFilesAsync();
            if (!saveTxtResult.IsSuccess)
                return saveTxtResult;

            var zipTxtResult = await TxtSaver.ZipTxtDbAsync();
            if (!zipTxtResult.IsSuccess)
                return zipTxtResult;

            _transactionsViewModel.Model.IsCollectionChanged = false;

            var deleteTxtResult = TxtSaver.DeleteTxtFiles();
            if (!deleteTxtResult.IsSuccess)
                return deleteTxtResult;

            return new LibResult(true, null);
        }


        private Task<LibResult> MapBack()
        {
            try
            {
                var bankAccounts = _keeperDataModel.AcMoDict.Values
                    .Where(a => a.IsBankAccount)
                    .Select(ac => Mapper.Map<BankAccount>(ac.BankAccount))
                    .ToList();
                var deposits = _keeperDataModel.AcMoDict.Values
                    .Where(a => a.IsDeposit)
                    .Select(ac => ac.BankAccount.Deposit)
                    .ToList();
                var cards = _keeperDataModel.AcMoDict.Values
                    .Where(a => a.IsCard)
                    .Select(ac => ac.BankAccount.PayCard)
                    .ToList();

                var exchangeRates = _keeperDataModel.ExchangeRates.Values.OrderBy(r => r.Date).ToList();
                // int i = 0;
                // exchangeRates.ForEach(r => r.Id = ++i);

                var bin = new KeeperBin
                {
                    OfficialRates = _keeperDataModel.OfficialRates.Values.ToList(),
                    ExchangeRates = exchangeRates,
                    MetalRates = _keeperDataModel.MetalRates,
                    RefinancingRates = _keeperDataModel.RefinancingRates,

                    TrustAccounts = _keeperDataModel.TrustAccounts,
                    TrustAssets = _keeperDataModel.InvestmentAssets.Select(a => a.Map()).ToList(),
                    TrustAssetRates = _keeperDataModel.AssetRates,
                    TrustTransactions = _keeperDataModel.InvestTranModels.Select(t => t.Map()).ToList(),

                    AccountPlaneList = _keeperDataModel.FlattenAccountTree().ToList(),
                    BankAccounts = bankAccounts,
                    Deposits = deposits,
                    PayCards = cards,

                    Transactions = _keeperDataModel.Transactions.Values.Select(t => t.Map()).ToList(),

                    DepositOffers = _keeperDataModel.DepositOffers.Select(o => o.Map()).ToList(),

                    Cars = _keeperDataModel.Cars.Select(c => Mapper.Map<Car>(c)).ToList(),
                    CarYearMileages = _keeperDataModel.Cars.SelectMany(c => c.YearsMileage).Select(y => Mapper.Map<CarYearMileage>(y)).ToList(),

                    Fuellings = _keeperDataModel.FuellingVms.Select(f => f.Map()).ToList(),
                    CardBalanceMemos = _keeperDataModel.CardBalanceMemoModels.Select(m => m.Map()).ToList(),
                };

                bin.DepositRateLines = new List<DepositRateLine>();
                bin.DepositConditions = new List<DepositConditions>();
                foreach (var depositOffer in _keeperDataModel.DepositOffers)
                {
                    foreach (var pair in depositOffer.CondsMap)
                    {
                        bin.DepositRateLines.AddRange(pair.Value.RateLines);
                        bin.DepositConditions.Add(Mapper.Map<DepositConditions>(pair.Value));
                    }
                }

                bin.ButtonCollections = _keeperDataModel.ButtonCollections.Select(c => c.Map()).ToList();
                bin.SalaryChanges = _keeperDataModel.SalaryChanges;
                bin.LargeExpenseThresholds = _keeperDataModel.LargeExpenseThresholds;

                return Task.FromResult(new LibResult(true, bin));
            }
            catch (Exception e)
            {
                return Task.FromResult(new LibResult(e, "MapBack"));
            }
        }

    }
}