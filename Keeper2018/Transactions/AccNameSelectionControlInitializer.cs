using System.Collections.Generic;

namespace Keeper2018
{
    public class AccNameSelectionControlInitializer
    {
        #region Buttons Collections
        private static readonly Dictionary<string, string> ButtonsForExpense =
            new Dictionary<string, string> { ["мк"] = "Мой кошелек", ["биб"] = "БИБ Сберка Моцная", ["газ"] = "БГПБ Расчетная BYN", ["юк"] = "Юлин кошелек", };

        private static readonly Dictionary<string, string> ButtonsForExpenseTags =
            new Dictionary<string, string>
            {
                ["pro"] = "Простор",
                ["евр"] = "Евроопт",
                ["рад"] = "Радзивиловский",
                ["бмр"] = "Белмаркет",
                ["ома"] = "Ома",
                ["маг"] = "Прочие магазины",
                ["еда"] = "Продукты в целом",
                ["стр"] = "Строительство дома",
                ["др"] = "Прочие расходы",
            };


        private static readonly Dictionary<string, string> ButtonsForIncome =
            new Dictionary<string, string> { ["зпл"] = "БИБ Зарплатная 0117-0122", ["шкф"] = "Шкаф", ["юк"] = "Юлин кошелек", };

        private static readonly Dictionary<string, string> ButtonsForIncomeTags =
            new Dictionary<string, string> { ["иит"] = "ИИТ", ["биб"] = "БИБ", ["газ"] = "БГПБ", ["%%"] = "Проценты по депозитам", };


        private static readonly Dictionary<string, string> ButtonsForTransfer =
            new Dictionary<string, string>
            {
                ["мк"] = "Мой кошелек",
                ["юк"] = "Юлин кошелек",
                ["биб"] = "БИБ Сберка Моцная",
                ["газ"] = "БГПБ Расчетная BYN",
                ["шкф"] = "Шкаф",
            };

        private static readonly Dictionary<string, string> ButtonsForTransferTags = new Dictionary<string, string>();


        private static readonly Dictionary<string, string> ButtonsForExchange =
            new Dictionary<string, string>
            {
                ["мк"] = "Мой кошелек",
                ["биб"] = "БИБ Сберка Моцная",
                ["газ"] = "БГПБ Расчетная BYN",
                ["биб$"] = "БИБ Вал шкат USD",
                ["газ$"] = "БГПБ Сберка USD",
            };

        private static readonly Dictionary<string, string> ButtonsForExchangeTags = new Dictionary<string, string>();
        #endregion

        private readonly ComboTreesCaterer _comboTreesCaterer;

        public AccNameSelectionControlInitializer(ComboTreesCaterer comboTreesCaterer)
        {
            _comboTreesCaterer = comboTreesCaterer;
        }

        public AccNameSelectorVm ForMyAccount(Transaction tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход: return Build("Куда", ButtonsForIncome, _comboTreesCaterer.MyAccNamesForIncome, tran.MyAccount.Header, "Шкаф");
                case OperationType.Расход: return Build("Откуда", ButtonsForExpense, _comboTreesCaterer.MyAccNamesForExpense, tran.MyAccount.Header, "Мой кошелек");
                case OperationType.Перенос: return Build("Откуда", ButtonsForTransfer, _comboTreesCaterer.MyAccNamesForTransfer, tran.MyAccount.Header, "Мой кошелек");
                case OperationType.Обмен:
                default:
                    return Build("Откуда", ButtonsForExchange, _comboTreesCaterer.MyAccNamesForExchange, tran.MyAccount.Header, "Мой кошелек");
            }
        }

        public AccNameSelectorVm ForMySecondAccount(Transaction tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Перенос: return Build("Куда", ButtonsForTransfer, _comboTreesCaterer.MyAccNamesForTransfer, tran.MySecondAccount?.Header, "Юлин кошелек");
                case OperationType.Обмен: return Build("Куда", ButtonsForExchange, _comboTreesCaterer.MyAccNamesForExchange, tran.MySecondAccount?.Header, "Мой кошелек");
                default: return Build("Куда", ButtonsForExchange, _comboTreesCaterer.MyAccNamesForExchange, "Мой кошелек", "Мой кошелек");
            }
        }
        public AccNameSelectorVm ForTags(Transaction tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход: return Build("Кто, за что", ButtonsForIncomeTags, _comboTreesCaterer.AccNamesForIncomeTags, tran.MyAccount.Header, "ИИТ");
                case OperationType.Расход: return Build("Кому, за что", ButtonsForExpenseTags, _comboTreesCaterer.AccNamesForExpenseTags, tran.MyAccount.Header, "Прочие расходы");
                case OperationType.Перенос: return Build("Теги", ButtonsForTransferTags, _comboTreesCaterer.AccNamesForTransferTags, tran.MyAccount.Header, "Форекс");
                case OperationType.Обмен:
                default:
                    return Build("Теги", ButtonsForExchangeTags, _comboTreesCaterer.AccNamesForExchangeTags, tran.MyAccount.Header, "БИБ");
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
