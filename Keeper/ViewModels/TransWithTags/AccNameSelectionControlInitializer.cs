using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.Controls.OneTranViewControls.SubControls.AccNameSelectionControl;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Extentions;
using Keeper.DomainModel.Trans;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.CommonKeeper;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    public class AccNameSelectionControlInitializer
    {
        #region Buttons Collections
        private static readonly Dictionary<string, string> ButtonsForExpense =
            new Dictionary<string, string> { ["мк"] = "Мой кошелек", ["биб"] = "БИБ Зарплатная 0117-0122",
                ["алф"] = "Карта покупок", ["юк"] = "Юлин кошелек", 
                ["бум"] = "Бумеранг 31/01/2023 3%", ["юбу"] = "Бумеранг Юля", };

        private static readonly Dictionary<string, string> ButtonsForExpenseTags =
            new Dictionary<string, string> { ["pro"] = "Простор", ["евр"] = "Евроопт", ["рад"] = "Радзивиловский", 
                ["ома"] = "Ома", ["маг"] = "Прочие магазины", ["еда"] = "Продукты в целом",
                ["лек"] = "Лекарства", ["стр"] = "Строительство дома", ["др"] = "Прочие расходы", };


        private static readonly Dictionary<string, string> ButtonsForIncome =
            new Dictionary<string, string> { ["зпл"] = "БИБ Зарплатная 0117-0122", ["шкф"] = "Шкаф", ["юк"] = "Юлин кошелек", };

        private static readonly Dictionary<string, string> ButtonsForIncomeTags =
            new Dictionary<string, string> { ["иит"] = "ИИТ", ["биб"] = "БИБ", ["газ"] = "БГПБ", ["%%"] = "Проценты по депозитам", };


        private static readonly Dictionary<string, string> ButtonsForTransfer =
            new Dictionary<string, string> { ["мк"] = "Мой кошелек", ["юк"] = "Юлин кошелек", ["биб"] = "БИБ Зарплатная 0117-0122",
                ["алф"] = "Карта покупок", ["шкф"] = "Шкаф", ["бум"] = "Бумеранг 31/01/2023 3%", ["юбу"] = "Бумеранг Юля", };

        private static readonly Dictionary<string, string> ButtonsForTransferTags = new Dictionary<string, string>();


        private static readonly Dictionary<string, string> ButtonsForExchange =
            new Dictionary<string, string> { ["мк"] = "Мой кошелек", ["биб"] = "БИБ Зарплатная 0117-0122", ["газ"] = "БГПБ Расчетная BYN",
                                                        ["биб$"] = "БИБ Visa USD 04/21", ["газ$"] = "БГПБ Сберка USD", };

        private static readonly Dictionary<string, string> ButtonsForExchangeTags = new Dictionary<string, string>();
        #endregion

        private readonly ComboTreesCaterer _comboTreesCaterer;

        [ImportingConstructor]
        public AccNameSelectionControlInitializer(ComboTreesCaterer comboTreesCaterer)
        {
            _comboTreesCaterer = comboTreesCaterer;
        }

        public AccNameSelectorVm ForMyAccount(TranWithTags tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход: return Build("Куда", ButtonsForIncome, _comboTreesCaterer.MyAccNamesForIncome, tran.MyAccount.Name, "Шкаф");
                case OperationType.Расход: return Build("Откуда", ButtonsForExpense, _comboTreesCaterer.MyAccNamesForExpense, tran.MyAccount.Name, "Мой кошелек");
                case OperationType.Перенос: return Build("Откуда", ButtonsForTransfer, _comboTreesCaterer.MyAccNamesForTransfer, tran.MyAccount.Name, "Мой кошелек");
                case OperationType.Обмен:
                default:
                    return Build("Откуда", ButtonsForExchange, _comboTreesCaterer.MyAccNamesForExchange, tran.MyAccount.Name, "Мой кошелек");
            }
        }

        public AccNameSelectorVm ForMySecondAccount(TranWithTags tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Перенос: return Build("Куда", ButtonsForTransfer, _comboTreesCaterer.MyAccNamesForTransfer, tran.MySecondAccount?.Name, "Юлин кошелек");
                case OperationType.Обмен: return Build("Куда", ButtonsForExchange, _comboTreesCaterer.MyAccNamesForExchange, tran.MySecondAccount?.Name, "Мой кошелек");
                default: return Build("Куда", ButtonsForExchange, _comboTreesCaterer.MyAccNamesForExchange, "Мой кошелек", "Мой кошелек");
            }
        }
        public AccNameSelectorVm ForTags(TranWithTags tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход: return Build("Кто, за что", ButtonsForIncomeTags, _comboTreesCaterer.AccNamesForIncomeTags, tran.MyAccount.Name, "ИИТ");
                case OperationType.Расход: return Build("Кому, за что", ButtonsForExpenseTags, _comboTreesCaterer.AccNamesForExpenseTags, tran.MyAccount.Name, "Прочие расходы");
                case OperationType.Перенос: return Build("Теги", ButtonsForTransferTags, _comboTreesCaterer.AccNamesForTransferTags, tran.MyAccount.Name, "Форекс");
                case OperationType.Обмен:
                default:
                    return Build("Теги", ButtonsForExchangeTags, _comboTreesCaterer.AccNamesForExchangeTags, tran.MyAccount.Name, "БИБ");
            }
        }

        public AccNameSelectorVm ForFilter()
        {

            return Build("", new Dictionary<string, string>(), _comboTreesCaterer.AccNamesForFilterTags, "Прочие расходы", "");
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
