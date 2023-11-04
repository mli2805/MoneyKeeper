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

        public async Task<bool> Save()
        {
            try
            {
                if (_shellPartsBinder.IsBusy) return false;
                _shellPartsBinder.IsBusy = true;
                _shellPartsBinder.FooterVisibility = Visibility.Visible;

                var bin = MapBack();

                var result3 = await BinSerializer.Serialize(bin);
                if (!result3.IsSuccess)
                {
                    MessageBox.Show(result3.Exception.Message);
                    return false;
                }

                var result = await bin.SaveAllToNewTxtAsync();
                if (result.IsSuccess)
                {
                    if (await TxtSaver.ZipTxtDbAsync())
                    {
                        var result2 = TxtSaver.DeleteTxtFiles();
                        if (!result2.IsSuccess)
                        {
                            MessageBox.Show(result2.Exception.Message);
                            return false;
                        }
                    }
                }
                else
                {
                    MessageBox.Show(result.Exception.Message);
                    return false;
                }

                _transactionsViewModel.Model.IsCollectionChanged = false;
            }
            catch (Exception e)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, $"Exception during dataModel saving: {e.Message}");
                _windowManager.ShowDialog(vm);
                return false;
            }
            finally
            {
                _shellPartsBinder.FooterVisibility = Visibility.Collapsed;
                _shellPartsBinder.IsBusy = false;
            }

            return true;
        }


        private KeeperBin MapBack()
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
            int i = 0;
            exchangeRates.ForEach(r => r.Id = ++i);

            var bin = new KeeperBin
            {
                OfficialRates = _keeperDataModel.OfficialRates.Values.ToList(),
                ExchangeRates = exchangeRates,
                MetalRates = _keeperDataModel.MetalRates,
                RefinancingRates = _keeperDataModel.RefinancingRates,

                TrustAccounts = _keeperDataModel.TrustAccounts,
                InvestmentAssets = _keeperDataModel.InvestmentAssets.Select(a => a.Map()).ToList(),
                AssetRates = _keeperDataModel.AssetRates,
                InvestmentTransactions = _keeperDataModel.InvestTranModels.Select(t => t.Map()).ToList(),

                AccountPlaneList = _keeperDataModel.FlattenAccountTree().ToList(),
                BankAccounts = bankAccounts,
                Deposits = deposits,
                PayCards = cards,

                Transactions = _keeperDataModel.Transactions.Values.Select(t => t.Map()).ToList(),

                DepositOffers = _keeperDataModel.DepositOffers.Select(o => o.Map()).ToList(),

                Cars = _keeperDataModel.Cars.Select(c => Mapper.Map<Car>(c)).ToList(),
                YearMileages = _keeperDataModel.Cars.SelectMany(c => c.YearsMileage).Select(y => Mapper.Map<YearMileage>(y)).ToList(),

                Fuellings = _keeperDataModel.FuellingVms.Select(f => f.Map()).ToList(),
            };

            bin.DepositRateLines = new List<DepositRateLine>();
            bin.DepoNewConds = new List<DepoNewConds>();
            foreach (var depositOffer in _keeperDataModel.DepositOffers)
            {
                foreach (var pair in depositOffer.CondsMap)
                {
                    bin.DepositRateLines.AddRange(pair.Value.RateLines);
                    bin.DepoNewConds.Add(Mapper.Map<DepoNewConds>(pair.Value));
                }
            }

            bin.ButtonCollections = _keeperDataModel.ButtonCollections.Select(c => c.Map()).ToList();

            return bin;
        }

    }
}