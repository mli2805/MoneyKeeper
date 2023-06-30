using System.Windows;
using Caliburn.Micro;

namespace Keeper2018
{
    public class OneAccountViewModel : Screen
    {
        private readonly ComboTreesProvider _comboTreesProvider;
        private readonly AccNameSelector _accNameSelectorForAssociations;
        private bool _isInAddMode;
        private string _oldName;
        public AccountItemModel AccountItemInWork { get; set; }
        public string ParentFolder { get; set; }

        private AccNameSelectorVm _myAccNameSelectorVm = new AccNameSelectorVm();
        public AccNameSelectorVm MyAccNameSelectorVm
        {
            get => _myAccNameSelectorVm;
            set
            {
                if (Equals(value, _myAccNameSelectorVm)) return;
                _myAccNameSelectorVm = value;
                NotifyOfPropertyChange();
            }
        }

        private AccNameSelectorVm _myAccNameSelectorVm2 = new AccNameSelectorVm();
        public AccNameSelectorVm MyAccNameSelectorVm2
        {
            get => _myAccNameSelectorVm2;
            set
            {
                if (Equals(value, _myAccNameSelectorVm2)) return;
                _myAccNameSelectorVm2 = value;
                NotifyOfPropertyChange();
            }
        }

        public Visibility SelectorVisibility { get; set; }
        public Visibility SelectorVisibility2 { get; set; }
        public Visibility TextVisibility { get; set; }

        public bool IsSavePressed { get; set; }

        public string TextIn { get; set; }

        public OneAccountViewModel(ComboTreesProvider comboTreesProvider, AccNameSelector accNameSelectorForAssociations)
        {
            _comboTreesProvider = comboTreesProvider;
            _accNameSelectorForAssociations = accNameSelectorForAssociations;
        }

        public void Initialize(AccountItemModel accountInWork, bool isInAddMode)
        {
            IsSavePressed = false;
            AccountItemInWork = accountInWork;
            _isInAddMode = isInAddMode;

            ParentFolder = AccountItemInWork.Parent == null ? "Корневой счет" : AccountItemInWork.Parent.Name;
            TextIn = AccountItemInWork.IsMyAccountsInBanksFolder ? "В банке" : "В папке";
            _oldName = accountInWork.Name;

            InitializeAccNameSelectors();
        }

        private void InitializeAccNameSelectors()
        {
            _comboTreesProvider.Initialize();

            if (AccountItemInWork.IsMyAccount)
            {
                MyAccNameSelectorVm.Visibility = Visibility.Collapsed;
                MyAccNameSelectorVm2.Visibility = Visibility.Collapsed;
                TextVisibility = Visibility.Collapsed;
            }
            else if (AccountItemInWork.IsTag)
            {
                MyAccNameSelectorVm.Visibility = Visibility.Visible;
                MyAccNameSelectorVm2.Visibility = Visibility.Collapsed;
                TextVisibility = Visibility.Visible;
                MyAccNameSelectorVm = _accNameSelectorForAssociations
                    .InitializeForAssociation(AccountItemInWork.Is(185)
                        ? AssociationEnum.ExternalForIncome
                        : AssociationEnum.ExternalForExpense, AccountItemInWork.AssociatedExternalId);
            }
            else
            {
                MyAccNameSelectorVm.Visibility = Visibility.Visible;
                MyAccNameSelectorVm2.Visibility = Visibility.Visible;
                TextVisibility = Visibility.Visible;
                MyAccNameSelectorVm = _accNameSelectorForAssociations
                    .InitializeForAssociation(AssociationEnum.IncomeForExternal, AccountItemInWork.AssociatedIncomeId);
                MyAccNameSelectorVm2 = _accNameSelectorForAssociations
                    .InitializeForAssociation(AssociationEnum.ExpenseForExternal, AccountItemInWork.AssociatedExpenseId);
            }
        }

        protected override void OnViewLoaded(object view)
        {
            var cap = _isInAddMode ? "Добавить" : "Изменить";
            DisplayName = $"{cap} (id = {AccountItemInWork.Id})";
        }

        public void Save()
        {
            ApplyAssociation();
            IsSavePressed = true;
            TryClose();
        }

        private void ApplyAssociation()
        {
            if (AccountItemInWork.IsMyAccount)
            {
            }
            else if (AccountItemInWork.IsTag)
            {
                AccountItemInWork.AssociatedExternalId = MyAccNameSelectorVm.MyAccName?.Id ?? 0;
            }
            else
            {
                AccountItemInWork.AssociatedIncomeId = MyAccNameSelectorVm.MyAccName?.Id ?? 0;
                AccountItemInWork.AssociatedExpenseId = MyAccNameSelectorVm2.MyAccName?.Id ?? 0;
            }
        }

        public void Cancel()
        {
            if (!_isInAddMode)
            {
                AccountItemInWork.Name = _oldName;
            }
            TryClose();
        }
    }
}
