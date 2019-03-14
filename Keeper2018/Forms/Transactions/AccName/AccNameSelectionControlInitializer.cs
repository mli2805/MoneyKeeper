using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    /// <summary>
    /// just for information
    /// 
    /// 162 ;       Мой кошелек 
    /// 163 ;       Юлин кошелек
    /// 167 ;       Шкаф
    /// 
    /// 651 ;         БИБ Зарплатная 0117-0122
    /// 675 ;         БГПБ Расчетная BYN
    /// 699 ;         Карта покупок
    /// 732 ;         Бумеранг 31/01/2023 3%
    /// 735 ;         Бумеранг Юля
    /// 690 ;         БИБ Visa USD 04/21
    /// 504 ;         БГПБ Сберка USD
    /// 
    /// 339 ;     БИБ
    /// 401 ;     БГПБ
    /// 
    /// 443 ;       ИИТ
    /// 
    /// 249 ;       Простор
    /// 523 ;       Евроопт
    /// 744 ;       Виталюр 
    /// 532 ;       Радзивиловский
    /// 291 ;       Ома
    /// 252 ;       Прочие магазины
    /// 
    /// 208 ;     Проценты по депозитам
    /// 701 ;     Кэшбэк
    /// 579 ;     Форекс
    /// 
    /// 257 ;     Продукты в целом 
    /// 589 ;     Строительство дома
    /// 199 ;     Лекарства
    /// 256 ;   Прочие расходы 
    /// </summary>

    public class AccNameSelectionControlInitializer
    {
        #region Buttons Collections
        private static readonly Dictionary<string, int> ButtonsForExpense =
            new Dictionary<string, int> { ["мк"] = 162, ["биб"] = 651,
                ["алф"] = 699, ["юк"] = 163, ["бум"] = 732, ["юбу"] = 735, };

        private static readonly Dictionary<string, int> ButtonsForExpenseTags =
            new Dictionary<string, int> { ["pro"] = 249, ["евр"] = 523, ["вит"] = 744,  ["рад"] = 532, 
                ["ома"] = 291, ["маг"] = 252, ["еда"] = 257, ["лек"] = 199, ["стр"] = 589, ["др"] = 256, };

        private static readonly Dictionary<string, int> ButtonsForIncome =
            new Dictionary<string, int> { ["зпл"] = 651, ["шкф"] = 167, ["юк"] = 163, };

        private static readonly Dictionary<string, int> ButtonsForIncomeTags =
            new Dictionary<string, int> { ["иит"] = 443, ["биб"] = 339, ["газ"] = 401, ["%%"] = 208, ["бэк"] = 701};

        private static readonly Dictionary<string, int> ButtonsForTransfer =
            new Dictionary<string, int> { ["мк"] = 162, ["юк"] = 163, ["биб"] = 651,
                ["алф"] = 699, ["шкф"] = 167, ["бум"] = 732, ["юбу"] = 735, };

        private static readonly Dictionary<string, int> ButtonsForTransferTags = new Dictionary<string, int>();

        private static readonly Dictionary<string, int> ButtonsForExchange =
            new Dictionary<string, int> { ["мк"] = 162, ["биб"] = 651, ["газ"] = 675, ["биб$"] = 690, ["газ$"] = 504, };

        private static readonly Dictionary<string, int> ButtonsForExchangeTags = new Dictionary<string, int>();
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
                case OperationType.Доход: return Build("Куда", ButtonsForIncome, 
                    _comboTreesProvider.MyAccNamesForIncome, tran.MyAccount.Id, 167);
                case OperationType.Расход: return Build("Откуда", ButtonsForExpense, 
                    _comboTreesProvider.MyAccNamesForExpense, tran.MyAccount.Id, 162);
                case OperationType.Перенос: return Build("Откуда", ButtonsForTransfer, 
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
                case OperationType.Перенос: return Build("Куда", ButtonsForTransfer, 
                    _comboTreesProvider.MyAccNamesForTransfer, tran.MySecondAccount?.Id ?? 0, 163);
                case OperationType.Обмен: return Build("Куда", ButtonsForExchange, 
                    _comboTreesProvider.MyAccNamesForExchange, tran.MySecondAccount?.Id ?? 0, 162);
                default: return Build("Куда", ButtonsForExchange,
                    _comboTreesProvider.MyAccNamesForExchange, 162, 162);
            }
        }
        public AccNameSelectorVm ForTags(TransactionModel tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход: return Build("Кто, за что", ButtonsForIncomeTags, 
                    _comboTreesProvider.AccNamesForIncomeTags, tran.MyAccount.Id, 443);
                case OperationType.Расход: return Build("Кому, за что", ButtonsForExpenseTags, 
                    _comboTreesProvider.AccNamesForExpenseTags, tran.MyAccount.Id, 256);
                case OperationType.Перенос: return Build("Теги", ButtonsForTransferTags, 
                    _comboTreesProvider.AccNamesForTransferTags, tran.MyAccount.Id, 579);
                // case OperationType.Обмен:
                default:
                    return Build("Теги", ButtonsForExchangeTags, _comboTreesProvider.AccNamesForExchangeTags, tran.MyAccount.Id, 339);
            }
        }

        public AccNameSelectorVm ForReceipt(int initialAccountId)
        {
            return Build("", ButtonsForExpenseTags, _comboTreesProvider.AccNamesForExpenseTags, initialAccountId, 0);
        }

        public AccNameSelectorVm ForFilter()
        {

            return Build("", new Dictionary<string, int>(), _comboTreesProvider.AccNamesForFilterTags, 256, 0);
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
    }
}
