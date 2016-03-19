using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.Controls.AccNameSelectionControl;
using Keeper.DomainModel.WorkTypes;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class MyAccNameSelectionControlInitializer
    {
        private static readonly Dictionary<string, string> ButtonsForExpense =
            new Dictionary<string, string> { ["мк"] = "Мой кошелек", ["биб"] = "БИБ Сберка Моцная", ["газ"] = "БГПБ Сберегательная", ["юк"] = "Юлин кошелек", };

        private static readonly Dictionary<string, string> ButtonsForExpenseTags =
            new Dictionary<string, string> { ["pro"] = "Простор", ["евр"] = "Евроопт", ["рад"] = "Радзивиловский", ["еда"] = "Продукты в целом", ["др"] = "Прочие расходы", };

        private static readonly Dictionary<string, string> ButtonsForIncome =
            new Dictionary<string, string> { ["зпл"] = "БИБ Зарплатная GOLD", ["шкф"] = "Шкаф", ["юк"] = "Юлин кошелек", };

        private static readonly Dictionary<string, string> ButtonsForIncomeTags =
            new Dictionary<string, string> { ["иит"] = "ИИТ", ["биб"] = "БИБ", ["газ"] = "БГПБ", ["%%"] = "Проценты по депозитам", };
        public AccNameSelectorVm ForExpense(string activeAccountName)
        {
            return Build("Откуда", ButtonsForExpense, ListsForComboTrees.MyAccNamesForExpense, activeAccountName, "Мой кошелек");
        }

        public AccNameSelectorVm ForExpenseTags(string activeAccountName)
        {
            return Build("Кому, за что", ButtonsForExpenseTags, ListsForComboTrees.AccNamesForExpenseTags, activeAccountName, "Прочие расходы");
        }

        public AccNameSelectorVm ForIncome(string activeAccountName)
        {
            return Build("Куда", ButtonsForIncome, ListsForComboTrees.MyAccNamesForIncome, activeAccountName, "Шкаф");
        }
        public AccNameSelectorVm ForIncomeTags(string activeAccountName)
        {
            return Build("Кто, за что", ButtonsForIncomeTags, ListsForComboTrees.AccNamesForIncomeTags, activeAccountName, "ИИТ");
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
