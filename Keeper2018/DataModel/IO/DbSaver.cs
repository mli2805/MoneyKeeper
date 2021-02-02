using System;
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
                    if (await DbTxtSaver.ZipTxtDbAsync())
                    {
                        var result2 = DbTxtSaver.DeleteTxtFiles();
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
            var result = new KeeperBin();

            _keeperDataModel.FlattenAccountTree();
            result.AccountPlaneList = _keeperDataModel.AccountPlaneList;
            result.Rates = _keeperDataModel.Rates;
            result.Transactions = _keeperDataModel.Transactions;
            result.DepositOffers = _keeperDataModel.DepositOffers;
            result.Cars = _keeperDataModel.Cars;
            result.Fuellings = _keeperDataModel.Fuellings;
            result.TagAssociations = _keeperDataModel.TagAssociations;

            return result;
        }
    }


}