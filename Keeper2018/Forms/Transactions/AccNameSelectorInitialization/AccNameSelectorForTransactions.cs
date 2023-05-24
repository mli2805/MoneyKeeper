using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public partial class AccNameSelector
    {
        public AccNameSelectorVm ForMyAccount(TransactionModel tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход:
                    return Build("Куда", 
                        _dataModel.ButtonCollections.First(c => c.Id == 1).ToButtonsDictionary(),
                        _comboTreesProvider.MyAccNamesForIncome, tran.MyAccount?.Id ?? 167);
                case OperationType.Расход:
                    return Build("Откуда", 
                        _dataModel.ButtonCollections.First(c => c.Id == 2).ToButtonsDictionary(),
                        _comboTreesProvider.MyAccNamesForExpense, tran.MyAccount?.Id ?? 162);
                case OperationType.Перенос:
                    return Build("Откуда", 
                        _dataModel.ButtonCollections.First(c => c.Id == 6).ToButtonsDictionary(),
                        _comboTreesProvider.MyAccNamesForTransfer, tran.MyAccount?.Id ?? 162);
                case OperationType.Обмен:
                default:
                    return Build("Откуда", 
                        _dataModel.ButtonCollections.First(c => c.Id == 7).ToButtonsDictionary(),
                        _comboTreesProvider.MyAccNamesForExchange, tran.MyAccount?.Id ?? 162);
            }
        }

        public AccNameSelectorVm ForMySecondAccount(TransactionModel tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Перенос:
                    return Build("Куда", 
                        _dataModel.ButtonCollections.First(c => c.Id == 6).ToButtonsDictionary(),
                        _comboTreesProvider.MyAccNamesForTransfer, tran.MySecondAccount?.Id ?? 163);
                case OperationType.Обмен:
                default:
                    return Build("Куда", 
                        _dataModel.ButtonCollections.First(c => c.Id == 7).ToButtonsDictionary(),
                        _comboTreesProvider.MyAccNamesForExchange, tran.MySecondAccount?.Id ?? 162);
            }
        }

        public AccNameSelectorVm ForMyNextAccount()
        {
            return Build("", null,
                _comboTreesProvider.MyAccNamesForTransfer, 912);
        }

        public AccNameSelectorVm ForTags(TransactionModel tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход:
                    return Build("Кто, за что", 
                        _dataModel.ButtonCollections.First(c => c.Id == 3).ToButtonsDictionary(),
                        _comboTreesProvider.AccNamesForIncomeTags, tran.Tags?.FirstOrDefault()?.Id ?? 443);
                case OperationType.Расход:
                    return Build("Кому, за что", 
                        _dataModel.ButtonCollections.First(c => c.Id == 4).ToButtonsDictionary(),
                        _comboTreesProvider.AccNamesForExpenseTags, tran.Tags?.FirstOrDefault()?.Id ?? 256);
                case OperationType.Перенос:
                    return Build("Теги", 
                        new Dictionary<string, int>(),
                        _comboTreesProvider.AccNamesForTransferTags, tran.Tags?.FirstOrDefault()?.Id ?? 579);
                case OperationType.Обмен:
                default:
                    return Build("Теги", 
                        new Dictionary<string, int>(),
                        _comboTreesProvider.AccNamesForExchangeTags, tran.Tags?.FirstOrDefault()?.Id ?? 339);
            }
        }

        public AccNameSelectorVm ForReceipt(int initialAccountId)
        {
            return Build("", 
                _dataModel.ButtonCollections.First(c => c.Id == 5).ToButtonsDictionary(),
                _comboTreesProvider.AccNamesForExpenseTags, initialAccountId);
        }

        public AccNameSelectorVm ForFilter()
        {

            return Build("", new Dictionary<string, int>(),
                _comboTreesProvider.AccNamesForFilterTags, 256);
        }



    }
}
