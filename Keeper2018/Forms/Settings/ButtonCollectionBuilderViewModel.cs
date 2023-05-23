using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class ButtonItem
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int AccountId { get; set; }
    }
    public class ButtonCollectionBuilderViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private readonly ComboTreesProvider _comboTreesProvider;

        public List<string> Collections { get; set; }
        public string SelectedCollection { get; set; }

        public List<AccName> AccNames { get; set; }
        public AccName SelectedAccName { get; set; }
        public string ShortName { get; set; }

        public List<ButtonItem> Buttons { get; set; }
        public ButtonItem SelectedButton { get; set; }

        public ButtonCollectionBuilderViewModel(KeeperDataModel dataModel, ComboTreesProvider comboTreesProvider)
        {
            _dataModel = dataModel;
            _comboTreesProvider = comboTreesProvider;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Настройка коллекций кнопок";
        }

        public void Initialize()
        {
            _comboTreesProvider.Initialize();

            Collections = new List<string>()
            {
                "Счета для получения средств",
                "Счета для расходования средств",

            };
            SelectedCollection = Collections.First();

            AccNames = ForMyAccount(OperationType.Доход);
            SelectedAccName = AccNames.First();
        }

        private List<AccName> ForMyAccount(OperationType operationType)
        {
            switch (operationType)
            {
                case OperationType.Доход:
                    return _comboTreesProvider.MyAccNamesForIncome;
                case OperationType.Расход:
                    return _comboTreesProvider.MyAccNamesForExpense;
                case OperationType.Перенос:
                    return _comboTreesProvider.MyAccNamesForTransfer;
                // case OperationType.Обмен:
                default:
                    return _comboTreesProvider.MyAccNamesForExchange;
            }
        }

        public void AddButtonToCollection(){}
    }
}
