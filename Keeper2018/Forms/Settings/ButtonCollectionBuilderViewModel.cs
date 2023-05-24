using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Microsoft.EntityFrameworkCore.Internal;

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
                Buttons = new ObservableCollection<AccountModel>(SelectedCollection.AccountModels);
                SelectedButton = Buttons.FirstOrDefault();
                AccNames = ForMyAccount(_selectedCollection, out AccName selectedAccName);
                SelectedAccName = selectedAccName;
                NotifyOfPropertyChange();
            }
        }

        private List<AccName> _accNames;
        public List<AccName> AccNames
        {
            get => _accNames;
            set
            {
                if (Equals(value, _accNames)) return;
                _accNames = value;
                SelectedAccName = AccNames.FirstOrDefault();
                NotifyOfPropertyChange();
            }
        }

        private AccName _selectedAccName;
        public AccName SelectedAccName
        {
            get => _selectedAccName;
            set
            {
                if (Equals(value, _selectedAccName)) return;
                _selectedAccName = value;
                NotifyOfPropertyChange();
            }
        }


        private ObservableCollection<AccountModel> _buttons;
        public ObservableCollection<AccountModel> Buttons
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

            Buttons = new ObservableCollection<AccountModel>(SelectedCollection.AccountModels);
            SelectedButton = Buttons.FirstOrDefault();

            _accNames = ForMyAccount(SelectedCollection, out _selectedAccName);
        }


        private List<AccName> ForMyAccount(ButtonCollectionModel collectionModel, out AccName selectedAccName)
        {
            switch (collectionModel.Id)
            {
                case 1:
                    selectedAccName = _comboTreesProvider.MyAccNamesForIncome.FindThroughTheForestById(695);
                    return _comboTreesProvider.MyAccNamesForIncome;
                case 2:
                    selectedAccName = _comboTreesProvider.MyAccNamesForExpense.FindThroughTheForestById(781);
                    return _comboTreesProvider.MyAccNamesForExpense;
                case 3:
                case 8:
                    selectedAccName = _comboTreesProvider.AccNamesForIncomeTags.FindThroughTheForestById(443);
                    return _comboTreesProvider.AccNamesForIncomeTags;
                case 4:
                case 5:
                case 9:
                    selectedAccName = _comboTreesProvider.AccNamesForExpenseTags.FindThroughTheForestById(256);
                    return _comboTreesProvider.AccNamesForExpenseTags;
                case 6:
                    selectedAccName = _comboTreesProvider.MyAccNamesForTransfer.FindThroughTheForestById(695);
                    return _comboTreesProvider.MyAccNamesForTransfer;
                case 7:
                    selectedAccName = _comboTreesProvider.MyAccNamesForExchange.FindThroughTheForestById(841);
                    return _comboTreesProvider.MyAccNamesForExchange;
                case 10:
                case 11:
                    var external = _comboTreesProvider.GetFullBranch(157);
                    selectedAccName = external.FindThroughTheForestById(225);
                    return external;
                case 12:
                    selectedAccName = _comboTreesProvider.AccNamesForInvestment.FindThroughTheForestById(695);
                    return _comboTreesProvider.AccNamesForInvestment;
                default:
                    selectedAccName = null;
                    return null;
            }
        }


        public void AddButtonToCollection()
        {
            var accountModel = _dataModel.AcMoDict[SelectedAccName.Id];
            if (accountModel.ButtonName != SelectedAccName.ButtonName)
                accountModel.ButtonName = SelectedAccName.ButtonName;

            if (!SelectedCollection.AccountModels.Contains(accountModel))
            {
                SelectedCollection.AccountModels.Add(accountModel);
                Buttons.Add(accountModel);
            }
        }

        public void RemoveButtonFromCollection()
        {
            if (SelectedButton == null) return;

            SelectedCollection.AccountModels.Remove(SelectedButton);
            Buttons.Remove(SelectedButton);
        }

        public void MoveLineUp()
        {
            if (SelectedButton == Buttons.First()) return;

            var array = Buttons.ToArray();
            var indexOf = array.IndexOf(SelectedButton);


            var newCollection = new ObservableCollection<AccountModel>();
            for (int i = 0; i < indexOf - 1; i++)
            {
                newCollection.Add(array[i]);
            }

            newCollection.Add(SelectedButton);
            newCollection.Add(array[indexOf - 1]);

            for (int i = indexOf + 1; i < array.Length; i++)
            {
                newCollection.Add(array[i]);
            }

            Buttons = newCollection;
            SelectedCollection.AccountModels = newCollection.ToList();
        }
    }
}
