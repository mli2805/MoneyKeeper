using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class DbSaver
    {
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

                // var dbResult = await bin.SeedDbContext();
                // if (!dbResult.IsSuccess)
                // {
                //     MessageBox.Show(dbResult.Exception.Message);
                // }
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
            var deposits = _keeperDataModel.AcMoDict.Values
                .Where(a => a.Deposit != null)
                .Select(ac => ac.Deposit)
                .ToList();

            var bin = new KeeperBin
            {
                Rates = _keeperDataModel.Rates.Values.ToList(),
                MetalRates = _keeperDataModel.MetalRates,

                StockTickers = _keeperDataModel.StockTickers,
                TickerRates = _keeperDataModel.TickerRates,

                AccountPlaneList = _keeperDataModel.FlattenAccountTree().ToList(),
                Deposits = deposits,
                PayCards = deposits.Where(d => d.Card != null).Select(c => c.Card).ToList(),

                Transactions = _keeperDataModel.Transactions.Values.Select(t => t.Map()).ToList(),

                DepositOffers = _keeperDataModel.DepositOffers.Select(o => o.Map()).ToList(),

                Cars = _keeperDataModel.Cars.Select(c => c.Map()).ToList(),
                YearMileages = _keeperDataModel.Cars.SelectMany(c => c.YearsMileage).Select(y => y.Map()).ToList(),

                Fuellings = _keeperDataModel.FuellingVms.Select(f => f.Map()).ToList(),
            };

            bin.DepositRateLines = new List<DepositRateLine>();
            bin.DepoNewConds = new List<DepoNewConds>();
            foreach (var depositOffer in _keeperDataModel.DepositOffers)
            {
                foreach (var pair in depositOffer.CondsMap)
                {
                    bin.DepositRateLines.AddRange(pair.Value.RateLines);
                    bin.DepoNewConds.Add(pair.Value.Map());
                }
            }
            return bin;
        }

    }
}