using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{

    public class ButtonCollectionBuilderViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private readonly ComboTreesProvider _comboTreesProvider;

        public List<ButtonCollectionModel> Collections { get; set; }

        private ButtonCollectionModel _selectedCollection;
        public ButtonCollectionModel SelectedCollection
        {
            get => _selectedCollection;
            set
            {
                if (Equals(value, _selectedCollection)) return;
                _selectedCollection = value;
                Buttons = SelectedCollection.AccountModels;
                SelectedButton = Buttons.FirstOrDefault();
                NotifyOfPropertyChange();
            }
        }

        public List<AccName> AccNames { get; set; }
        public AccName SelectedAccName { get; set; }
        public string ShortName { get; set; }

        private List<AccountModel> _buttons;
        public List<AccountModel> Buttons
        {
            get => _buttons;
            set
            {
                if (Equals(value, _buttons)) return;
                _buttons = value;
                NotifyOfPropertyChange();
            }
        }

        private AccountModel _selectedButton;
        public AccountModel SelectedButton
        {
            get => _selectedButton;
            set
            {
                if (Equals(value, _selectedButton)) return;
                _selectedButton = value;
                NotifyOfPropertyChange();
            }
        }

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

            Collections = _dataModel.ButtonCollections;
            SelectedCollection = Collections.First();

            Buttons = SelectedCollection.AccountModels;
            SelectedButton = Buttons.FirstOrDefault();

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

      
        public void AddButtonToCollection() { }
    }
}
