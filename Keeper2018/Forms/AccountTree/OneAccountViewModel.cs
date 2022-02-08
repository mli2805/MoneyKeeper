using System.Windows;
using Caliburn.Micro;

namespace Keeper2018
{
    public class OneAccountViewModel : Screen
    {
        private readonly ComboTreesProvider _comboTreesProvider;
        private readonly AccNameSelectionControlInitializer _accNameSelectionControlInitializer;
        private bool _isInAddMode;
        private string _oldName;
        public AccountModel AccountInWork { get; set; }
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

        public OneAccountViewModel(ComboTreesProvider comboTreesProvider,
            AccNameSelectionControlInitializer accNameSelectionControlInitializer)
        {
            _comboTreesProvider = comboTreesProvider;
            _accNameSelectionControlInitializer = accNameSelectionControlInitializer;
        }

        public void Initialize(AccountModel accountInWork, bool isInAddMode)
        {
            IsSavePressed = false;
            AccountInWork = accountInWork;
            _isInAddMode = isInAddMode;

            ParentFolder = AccountInWork.Owner == null ? "Корневой счет" : AccountInWork.Owner.Name;
            _oldName = accountInWork.Name;

            InitializeAccNameSelectors();
        }

        private void InitializeAccNameSelectors()
        {
            _comboTreesProvider.Initialize();

            if (AccountInWork.IsMyAccount)
            {
                MyAccNameSelectorVm.Visibility = Visibility.Collapsed;
                MyAccNameSelectorVm2.Visibility = Visibility.Collapsed;
                TextVisibility = Visibility.Collapsed;
            }
            else if (AccountInWork.IsTag)
            {
                MyAccNameSelectorVm.Visibility = Visibility.Visible;
                MyAccNameSelectorVm2.Visibility = Visibility.Collapsed;
                TextVisibility = Visibility.Visible;
                _accNameSelectionControlInitializer
                    .ForAssociation(MyAccNameSelectorVm,
                                    AccountInWork.Is(185)
                                        ? AssociationEnum.ExternalForIncome
                                        : AssociationEnum.ExternalForExpense,
                                    AccountInWork.AssociatedExternalId);
            }
            else
            {
                MyAccNameSelectorVm.Visibility = Visibility.Visible;
                MyAccNameSelectorVm2.Visibility = Visibility.Visible;
                TextVisibility = Visibility.Visible;
                _accNameSelectionControlInitializer
                    .ForAssociation(MyAccNameSelectorVm, AssociationEnum.IncomeForExternal, AccountInWork.AssociatedIncomeId);
                _accNameSelectionControlInitializer
                    .ForAssociation(MyAccNameSelectorVm2, AssociationEnum.ExpenseForExternal, AccountInWork.AssociatedExpenseId);
            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _isInAddMode ? "Добавить" : "Изменить";
        }

        public void Save()
        {
            ApplyAssociation();
            IsSavePressed = true;
            TryClose();
        }

        private void ApplyAssociation()
        {
            if (AccountInWork.IsMyAccount)
            {
            }
            else if (AccountInWork.IsTag)
            {
                AccountInWork.AssociatedExternalId = MyAccNameSelectorVm.MyAccName?.Id ?? 0;
            }
            else
            {
                AccountInWork.AssociatedIncomeId = MyAccNameSelectorVm.MyAccName?.Id ?? 0;
                AccountInWork.AssociatedExpenseId = MyAccNameSelectorVm2.MyAccName?.Id ?? 0;
            }
        }

        public void Cancel()
        {
            if (!_isInAddMode)
            {
                AccountInWork.Header = _oldName;
            }
            TryClose();
        }
    }
}
