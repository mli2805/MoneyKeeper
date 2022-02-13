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
                ["шоп"] = 878,
            };

        private static readonly Dictionary<string, int> ButtonsForExpenseTags =
            new Dictionary<string, int>
            {
                ["pro"] = 249,
                ["евр"] = 523,
                ["вит"] = 744,
                ["гпп"] = 824,
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
                ["шоп"] = 878,
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
                ["шоп"] = 878,
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
            new Dictionary<string, int> { ["з.п"] = 204, ["юфр"] = 314, ["%%"] = 208, ["бэк"] = 701 };

        private static readonly Dictionary<string, int> ButtonsExpensesForExternal =
            new Dictionary<string, int> { ["еда"] = 257, ["обе"] = 193, ["с/х"] = 446, ["дпр"] = 362, ["лек"] = 199, ["леч"] = 354, ["гад"] = 751, ["др"] = 256, };

        private static readonly Dictionary<string, int> ButtonsExternalForIncome =
                  new Dictionary<string, int> { ["фсзн"] = 177, ["род"] = 225, };

        private static readonly Dictionary<string, int> ButtonsExternalForExpense =
                         new Dictionary<string, int> { ["нал"] = 520, ["рикз"] = 668, ["род"] = 225, };
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
                case AssociationEnum.IncomeForExternal:
                    Initialize(selector, "Для дохода", ButtonsIncomesForExternal,
                        _comboTreesProvider.GetFullBranch(185), selectedId, 0); return;
                case AssociationEnum.ExpenseForExternal:
                    Initialize(selector, "Для расхода", ButtonsExpensesForExternal,
                        _comboTreesProvider.GetFullBranch(189), selectedId, 0); return;
                case AssociationEnum.ExternalForIncome:
                    Initialize(selector, "Контрагент", ButtonsExternalForIncome,
                        _comboTreesProvider.GetFullBranch(157), selectedId, 0); return;
                // case AssociationEnum.externalForExpense:
                default:
                    Initialize(selector, "Контрагент", ButtonsExternalForExpense,
                        _comboTreesProvider.GetFullBranch(157), selectedId, 0); return;
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
}
