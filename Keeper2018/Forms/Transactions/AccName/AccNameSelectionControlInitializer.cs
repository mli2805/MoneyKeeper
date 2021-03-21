using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public class AccNameSelectionControlInitializer
    {
        #region Buttons Collections
        private static readonly Dictionary<string, int> ButtonsForExpense =
            new Dictionary<string, int>
            {
                ["мк"] = 162,
                ["юк"] = 163,
                ["шка"] = 167,
                ["бум"] = 732,
                ["юбу"] = 735,
                ["джо"] = 781,
                ["каш"] = 776,
                ["шоп"] = 785,
            };

        private static readonly Dictionary<string, int> ButtonsForExpenseTags =
            new Dictionary<string, int>
            {
                ["pro"] = 249,
                ["евр"] = 523,
                ["вит"] = 744,
                ["рад"] = 532,
                ["ома"] = 291,
                ["маг"] = 252,
                ["еда"] = 257,
                ["лек"] = 199,
                ["стр"] = 589,
                ["др"] = 256,
            };

        private static readonly Dictionary<string, int> ButtonsForReceiptTags =
            new Dictionary<string, int> { ["еда"] = 257, ["c/x"] = 446, ["др"] = 256, ["др-д"] = 362, };

        private static readonly Dictionary<string, int> ButtonsForIncome =
            new Dictionary<string, int>
            {
                ["алф"] = 695,
                ["шкф"] = 167,
                ["юк"] = 163,
                ["бум"] = 732,
                ["юбу"] = 735,
                ["джо"] = 781,
                ["каш"] = 776,
                ["шоп"] = 785,
            };

        private static readonly Dictionary<string, int> ButtonsForIncomeTags =
            new Dictionary<string, int> { ["иит"] = 443, ["биб"] = 339, ["газ"] = 401, ["%%"] = 208, ["бэк"] = 701 };

        private static readonly Dictionary<string, int> ButtonsForTransfer =
            new Dictionary<string, int>
            {
                ["мк"] = 162,
                ["юк"] = 163,
                ["джо"] = 781,
                ["алф"] = 695,
                ["шкф"] = 167,
                ["бум"] = 732,
                ["юбу"] = 735,
                ["каш"] = 776,
                ["шоп"] = 785,
            };

        private static readonly Dictionary<string, int> ButtonsForTransferTags = new Dictionary<string, int>();

        private static readonly Dictionary<string, int> ButtonsForExchange =
            new Dictionary<string, int>
            {
                ["мк"] = 162,
                ["джо"] = 781,
                ["биб$"] = 690,
                ["газ"] = 675,
                ["газ$"] = 504,
                ["бум"] = 732,
                ["при$"] = 733,
            };

        private static readonly Dictionary<string, int> ButtonsForExchangeTags = new Dictionary<string, int>();

        private static readonly Dictionary<string, int> ButtonsIncomesForExternal =
            new Dictionary<string, int> { ["%%"] = 208, ["бэк"] = 701 };

        private static readonly Dictionary<string, int> ButtonsExpensesForExternal =
            new Dictionary<string, int> { ["еда"] = 257, ["лек"] = 199, ["др"] = 256, };


        #endregion

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
                    return Build("Куда", ButtonsForIncome,
                            _comboTreesProvider.MyAccNamesForIncome, tran.MyAccount.Id, 167);
                case OperationType.Расход:
                    return Build("Откуда", ButtonsForExpense,
                            _comboTreesProvider.MyAccNamesForExpense, tran.MyAccount.Id, 162);
                case OperationType.Перенос:
                    return Build("Откуда", ButtonsForTransfer,
                            _comboTreesProvider.MyAccNamesForTransfer, tran.MyAccount.Id, 162);
                // case OperationType.Обмен:
                default:
                    return Build("Откуда", ButtonsForExchange,
                        _comboTreesProvider.MyAccNamesForExchange, tran.MyAccount.Id, 162);
            }
        }

        public AccNameSelectorVm ForMySecondAccount(TransactionModel tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Перенос:
                    return Build("Куда", ButtonsForTransfer,
                        _comboTreesProvider.MyAccNamesForTransfer, tran.MySecondAccount?.Id ?? 0, 163);
                case OperationType.Обмен:
                    return Build("Куда", ButtonsForExchange,
                        _comboTreesProvider.MyAccNamesForExchange, tran.MySecondAccount?.Id ?? 0, 162);
                default:
                    return Build("Куда", ButtonsForExchange,
                        _comboTreesProvider.MyAccNamesForExchange, 162, 162);
            }
        }
        public AccNameSelectorVm ForTags(TransactionModel tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход:
                    return Build("Кто, за что", ButtonsForIncomeTags,
                        _comboTreesProvider.AccNamesForIncomeTags, tran.MyAccount.Id, 443);
                case OperationType.Расход:
                    return Build("Кому, за что", ButtonsForExpenseTags,
                        _comboTreesProvider.AccNamesForExpenseTags, tran.MyAccount.Id, 256);
                case OperationType.Перенос:
                    return Build("Теги", ButtonsForTransferTags,
                        _comboTreesProvider.AccNamesForTransferTags, tran.MyAccount.Id, 579);
                // case OperationType.Обмен:
                default:
                    return Build("Теги", ButtonsForExchangeTags,
                        _comboTreesProvider.AccNamesForExchangeTags, tran.MyAccount.Id, 339);
            }
        }

        public AccNameSelectorVm ForAssociationIncome(int myAccountId)
        {
            return Build("Title", ButtonsForIncomeTags, _comboTreesProvider.AccNamesForIncomeTags, myAccountId, 443);
        }

        public AccNameSelectorVm ForReceipt(int initialAccountId)
        {
            return Build("", ButtonsForReceiptTags,
                _comboTreesProvider.AccNamesForExpenseTags, initialAccountId, 0);
        }

        public AccNameSelectorVm ForFilter()
        {

            return Build("", new Dictionary<string, int>(),
                _comboTreesProvider.AccNamesForFilterTags, 256, 0);
        }

        private AccNameSelectorVm Build(string controlTitle, Dictionary<string, int> frequentAccountButtonNames,
                                            List<AccName> availableAccNames, int activeAccountId, int defaultAccountId)
        {
            return new AccNameSelectorVm
            {
                ControlTitle = controlTitle,
                Buttons = frequentAccountButtonNames.Select(
                    button => new AccNameButtonVm(button.Key,
                        availableAccNames.FindThroughTheForestById(button.Value))).ToList(),
                AvailableAccNames = availableAccNames,
                MyAccName = availableAccNames.FindThroughTheForestById(activeAccountId)
                            ?? availableAccNames.FindThroughTheForestById(defaultAccountId)

            };
        }


        public void ForAssociation(AccNameSelectorVm selector, AssociationEnum associationType, int selectedId)
        {
            switch (associationType)
            {
                case AssociationEnum.incomeForExternal:
                    Initialize(selector, "Для дохода", ButtonsIncomesForExternal,
                        _comboTreesProvider.GetFullBranch(185), selectedId, 0); return;
                case AssociationEnum.expenseForExternal:
                    Initialize(selector, "Для расхода", ButtonsExpensesForExternal,
                        _comboTreesProvider.GetFullBranch(189), selectedId, 285); return;
                case AssociationEnum.externalForIncome:
                    Initialize(selector, "Контрагент", ButtonsForIncomeTags,
                        _comboTreesProvider.GetFullBranch(157), selectedId, 171); return;
                // case AssociationEnum.externalForExpense:
                default:
                    Initialize(selector, "Контрагент", ButtonsForExpenseTags,
                        _comboTreesProvider.GetFullBranch(157), selectedId, 249); return;
            }
        }


        private void Initialize(AccNameSelectorVm selector,
            string controlTitle, Dictionary<string, int> frequentAccountButtonNames,
            List<AccName> availableAccNames, int activeAccountId, int defaultAccountId)
        {
            selector.ControlTitle = controlTitle;
            selector.Buttons = frequentAccountButtonNames.Select(
                button => new AccNameButtonVm(button.Key,
                    availableAccNames.FindThroughTheForestById(button.Value))).ToList();
            selector.AvailableAccNames = availableAccNames;
            selector.MyAccName = availableAccNames.FindThroughTheForestById(activeAccountId)
                                 ?? availableAccNames.FindThroughTheForestById(defaultAccountId);
        }
    }

    public enum AssociationEnum { incomeForExternal, expenseForExternal, externalForIncome, externalForExpense }
}
