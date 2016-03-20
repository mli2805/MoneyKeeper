using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.Controls.OneTranViewControls.SubControls.AccNameSelectionControl;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Transactions;
using Keeper.DomainModel.WorkTypes;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class AccNameSelectionControlInitializer
    {
        private static readonly Dictionary<string, string> ButtonsForExpense =
            new Dictionary<string, string> { ["мк"] = "Мой кошелек", ["биб"] = "БИБ Сберка Моцная", ["газ"] = "БГПБ Сберегательная", ["юк"] = "Юлин кошелек", };

        private static readonly Dictionary<string, string> ButtonsForExpenseTags =
            new Dictionary<string, string> { ["pro"] = "Простор", ["евр"] = "Евроопт", ["рад"] = "Радзивиловский", ["еда"] = "Продукты в целом", ["др"] = "Прочие расходы", };

        private static readonly Dictionary<string, string> ButtonsForIncome =
            new Dictionary<string, string> { ["зпл"] = "БИБ Зарплатная GOLD", ["шкф"] = "Шкаф", ["юк"] = "Юлин кошелек", };

        private static readonly Dictionary<string, string> ButtonsForIncomeTags =
            new Dictionary<string, string> { ["иит"] = "ИИТ", ["биб"] = "БИБ", ["газ"] = "БГПБ", ["%%"] = "Проценты по депозитам", };

        private static readonly Dictionary<string, string> ButtonsForTransfer =
            new Dictionary<string, string> { ["мк"] = "Мой кошелек", ["юк"] = "Юлин кошелек", ["биб"] = "БИБ Сберка Моцная", ["газ"] = "БГПБ Сберегательная", ["шкф"] = "Шкаф", };


        public AccNameSelectorVm ForMyAccount(TranWithTags tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход: return Build("Куда", ButtonsForIncome, ListsForComboTrees.MyAccNamesForIncome, tran.MyAccount.Name, "Шкаф");
                case OperationType.Расход: return Build("Откуда", ButtonsForExpense, ListsForComboTrees.MyAccNamesForExpense, tran.MyAccount.Name, "Мой кошелек");
                case OperationType.Перенос: return Build("Откуда", ButtonsForTransfer, ListsForComboTrees.MyAccNamesForTransfer, tran.MyAccount.Name, "Мой кошелек");
                default: return Build("Откуда", ButtonsForExpense, ListsForComboTrees.MyAccNamesForExpense, tran.MyAccount.Name, "Мой кошелек");
            }
        }

        public AccNameSelectorVm ForMySecondAccount(TranWithTags tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Перенос: return Build("Куда", ButtonsForTransfer, ListsForComboTrees.MyAccNamesForTransfer, tran.MySecondAccount.Name, "Юлин кошелек");
                default: return Build("Куда", ButtonsForTransfer, ListsForComboTrees.MyAccNamesForTransfer, tran.MyAccount.Name, "Юлин кошелек");
            }
        }
        public AccNameSelectorVm ForTags(TranWithTags tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход: return Build("Кто, за что", ButtonsForIncomeTags, ListsForComboTrees.AccNamesForIncomeTags, tran.MyAccount.Name, "ИИТ");
                case OperationType.Расход: return Build("Кому, за что", ButtonsForExpenseTags, ListsForComboTrees.AccNamesForExpenseTags, tran.MyAccount.Name, "Прочие расходы");
                default: return Build("Кому, за что", ButtonsForExpenseTags, ListsForComboTrees.AccNamesForExpenseTags, tran.MyAccount.Name, "Прочие расходы");
            }
        }

        private AccNameSelectorVm Build(string controlTitle, Dictionary<string, string> frequentAccountButtonNames,
                                            List<AccName> availableAccNames, string activeAccountName, string defaultAccountName)
        {
            return new AccNameSelectorVm
            {
                ControlTitle = controlTitle,
                Buttons = frequentAccountButtonNames.Select(
                    button => new AccNameButtonVm(button.Key,
                        availableAccNames.FindThroughTheForest(button.Value))).ToList(),
                AvailableAccNames = availableAccNames,
                MyAccName = availableAccNames.FindThroughTheForest(activeAccountName)
                            ?? availableAccNames.FindThroughTheForest(defaultAccountName)

            };
        }
    }
}
