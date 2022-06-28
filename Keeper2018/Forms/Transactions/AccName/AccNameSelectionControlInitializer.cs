using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public class AccNameSelectionControlInitializer
    {
        private readonly ComboTreesProvider _comboTreesProvider;

        public AccNameSelectionControlInitializer(ComboTreesProvider comboTreesProvider)
        {
            _comboTreesProvider = comboTreesProvider;
        }

        public AccNameSelectorVm ForMyAccount(TransactionModel tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход:
                    return Build("Куда", ButtonCollections.ForIncome,
                            _comboTreesProvider.MyAccNamesForIncome, tran.MyAccount?.Id ?? 167);
                case OperationType.Расход:
                    return Build("Откуда", ButtonCollections.ForExpense,
                            _comboTreesProvider.MyAccNamesForExpense, tran.MyAccount?.Id ?? 162);
                case OperationType.Перенос:
                    return Build("Откуда", ButtonCollections.ButtonsForTransfer,
                            _comboTreesProvider.MyAccNamesForTransfer, tran.MyAccount?.Id ?? 162);
                // case OperationType.Обмен:
                default:
                    return Build("Откуда", ButtonCollections.ForExchange,
                        _comboTreesProvider.MyAccNamesForExchange, tran.MyAccount?.Id ?? 162);
            }
        }

        public AccNameSelectorVm ForMySecondAccount(TransactionModel tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Перенос:
                    return Build("Куда", ButtonCollections.ButtonsForTransfer,
                        _comboTreesProvider.MyAccNamesForTransfer, tran.MySecondAccount?.Id ?? 163);
                case OperationType.Обмен:
                    return Build("Куда", ButtonCollections.ForExchange,
                        _comboTreesProvider.MyAccNamesForExchange, tran.MySecondAccount?.Id ?? 162);
                default:
                    return Build("Куда", ButtonCollections.ForExchange,
                        _comboTreesProvider.MyAccNamesForExchange, 162);
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
                    return Build("Кто, за что", ButtonCollections.ForIncomeTags,
                        _comboTreesProvider.AccNamesForIncomeTags, tran.Tags?.FirstOrDefault()?.Id ?? 443);
                case OperationType.Расход:
                    return Build("Кому, за что", ButtonCollections.ForExpenseTags,
                        _comboTreesProvider.AccNamesForExpenseTags, tran.Tags?.FirstOrDefault()?.Id ?? 256);
                case OperationType.Перенос:
                    return Build("Теги", ButtonCollections.ForTransferTags,
                        _comboTreesProvider.AccNamesForTransferTags, tran.Tags?.FirstOrDefault()?.Id ?? 579);
                // case OperationType.Обмен:
                default:
                    return Build("Теги", ButtonCollections.ForExchangeTags,
                        _comboTreesProvider.AccNamesForExchangeTags, tran.Tags?.FirstOrDefault()?.Id ?? 339);
            }
        }

        public AccNameSelectorVm ForReceipt(int initialAccountId)
        {
            return Build("", ButtonCollections.ForReceiptTags,
                _comboTreesProvider.AccNamesForExpenseTags, initialAccountId);
        }

        public AccNameSelectorVm ForFilter()
        {

            return Build("", new Dictionary<string, int>(),
                _comboTreesProvider.AccNamesForFilterTags, 256);
        }

        private AccNameSelectorVm Build(string controlTitle, Dictionary<string, int> frequentAccountButtonNames,
                                            List<AccName> availableAccNames, int activeAccountId)
        {
            return new AccNameSelectorVm
            {
                ControlTitle = controlTitle,
                Buttons = frequentAccountButtonNames == null 
                    ? new List<AccNameButtonVm>() 
                    : frequentAccountButtonNames.Select(
                        button => new AccNameButtonVm(button.Key,
                            availableAccNames.FindThroughTheForestById(button.Value))).ToList(),
                AvailableAccNames = availableAccNames,
                MyAccName = availableAccNames.FindThroughTheForestById(activeAccountId),
            };
        }
    }
}
