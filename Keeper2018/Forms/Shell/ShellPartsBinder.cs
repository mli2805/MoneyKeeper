using System;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class ShellPartsBinder : PropertyChangedBase
    {
        private readonly KeeperDataModel _keeperDataModel;

        private bool _isSelectedAccountUsedInTransaction;
        private AccountItemModel _selectedAccountItemModel;
        public AccountItemModel SelectedAccountItemModel
        {
            get => _selectedAccountItemModel;
            set
            {
                if (Equals(value, _selectedAccountItemModel)) return;
                _selectedAccountItemModel = value;
                _isSelectedAccountUsedInTransaction = UsedInTransaction(_selectedAccountItemModel.Id);
                NotifyOfPropertyChange();

                NotifyOfPropertyChange(nameof(IsEnabledAddIntoAccount));
                NotifyOfPropertyChange(nameof(IsEnabledDeleteAccount));

                NotifyOfPropertyChange(nameof(DepositMenuVisibility));
                NotifyOfPropertyChange(nameof(CardMenuVisibility));
                NotifyOfPropertyChange(nameof(MyLeafMenuVisibility));
                NotifyOfPropertyChange(nameof(MyFolderMenuVisibility));
                NotifyOfPropertyChange(nameof(MyLeafNotDepositMenuVisibility));
                NotifyOfPropertyChange(nameof(AddDepositVisibility));
            }
        }

        public Visibility DepositMenuVisibility => SelectedAccountItemModel.IsDeposit ? Visibility.Visible : Visibility.Collapsed;
        public Visibility CardMenuVisibility => SelectedAccountItemModel.IsCard ? Visibility.Visible : Visibility.Collapsed;
        public Visibility MyLeafMenuVisibility => SelectedAccountItemModel.IsLeaf && SelectedAccountItemModel.IsMyAccount 
            ? Visibility.Visible : Visibility.Collapsed;
        public Visibility MyFolderMenuVisibility => SelectedAccountItemModel.IsFolder && SelectedAccountItemModel.IsMyAccount 
            ? Visibility.Visible : Visibility.Collapsed;
        public Visibility MyLeafNotDepositMenuVisibility => SelectedAccountItemModel.IsLeaf 
                            && SelectedAccountItemModel.IsMyAccount && !SelectedAccountItemModel.IsDeposit 
            ? Visibility.Visible : Visibility.Collapsed;

        public Visibility AddDepositVisibility => SelectedAccountItemModel.IsMyAccountInBank 
        ? Visibility.Visible : Visibility.Collapsed;

        public bool IsEnabledAddIntoAccount => !_isSelectedAccountUsedInTransaction && !SelectedAccountItemModel.IsFolderOfClosed;
        public bool IsEnabledDeleteAccount => !SelectedAccountItemModel.IsFolder && !_isSelectedAccountUsedInTransaction;


        private BalanceOrTraffic _balanceOrTraffic;
        public BalanceOrTraffic BalanceOrTraffic    
        {
            get => _balanceOrTraffic;
            set
            {
                if (value == _balanceOrTraffic) return;
                _balanceOrTraffic = value;
                NotifyOfPropertyChange();
            }
        }

        public Period SelectedPeriod => _balanceOrTraffic == BalanceOrTraffic.Balance
                    ? new Period(new DateTime(2001, 12, 30), TranslatedDate)
                    : new Period(TranslatedPeriod.StartDate, TranslatedPeriod.FinishMoment);

        private Period _translatedPeriod;
        public Period TranslatedPeriod
        {
            get => _translatedPeriod;
            set
            {
                _translatedPeriod = value;
                NotifyOfPropertyChange(() => TranslatedPeriod);
                NotifyOfPropertyChange(() => SelectedPeriod);
            }
        }

        private DateTime _translatedDate;

        public DateTime TranslatedDate
        {
            get => _translatedDate.GetEndOfDate();
            set
            {
                _translatedDate = value;
                NotifyOfPropertyChange(() => TranslatedDate);
                NotifyOfPropertyChange(() => SelectedPeriod);
            }
        }

        private Visibility _footerVisibility = Visibility.Collapsed;

        public Visibility FooterVisibility
        {
            get => _footerVisibility;
            set
            {
                if (value == _footerVisibility) return;
                _footerVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private DateTime _justToForceBalanceRecalculation;
        public DateTime JustToForceBalanceRecalculation
        {
            get => _justToForceBalanceRecalculation;
            set
            {
                if (value.Equals(_justToForceBalanceRecalculation)) return;
                _justToForceBalanceRecalculation = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsBusy { get; set; }

        public ShellPartsBinder(KeeperDataModel keeperDataModel)
        {
            _keeperDataModel = keeperDataModel;
        }

        private bool UsedInTransaction(int accountId)
        {
            return _keeperDataModel.Transactions.Values.Any(t =>
                t.MyAccount.Id == accountId ||
                (t.MySecondAccount != null && t.MySecondAccount.Id == accountId) ||
                t.Tags != null && t.Tags.Select(tag => tag.Id).Contains(accountId));
        }
    }
}