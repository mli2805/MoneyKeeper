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
        private AccountModel _selectedAccountModel;
        public AccountModel SelectedAccountModel
        {
            get => _selectedAccountModel;
            set
            {
                if (Equals(value, _selectedAccountModel)) return;
                _selectedAccountModel = value;
                _isSelectedAccountUsedInTransaction = UsedInTransaction(_selectedAccountModel.Id);
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

        public Visibility DepositMenuVisibility => SelectedAccountModel.IsDeposit ? Visibility.Visible : Visibility.Collapsed;
        public Visibility CardMenuVisibility => SelectedAccountModel.IsCard ? Visibility.Visible : Visibility.Collapsed;
        public Visibility MyLeafMenuVisibility => SelectedAccountModel.IsLeaf && SelectedAccountModel.IsMyAccount 
            ? Visibility.Visible : Visibility.Collapsed;
        public Visibility MyFolderMenuVisibility => SelectedAccountModel.IsFolder && SelectedAccountModel.IsMyAccount 
            ? Visibility.Visible : Visibility.Collapsed;
        public Visibility MyLeafNotDepositMenuVisibility => SelectedAccountModel.IsLeaf 
                            && SelectedAccountModel.IsMyAccount && !SelectedAccountModel.IsDeposit 
            ? Visibility.Visible : Visibility.Collapsed;

        public Visibility AddDepositVisibility => SelectedAccountModel.IsMyAccountsInBanksFolder 
        ? Visibility.Visible : Visibility.Collapsed;

        public bool IsEnabledAddIntoAccount => !_isSelectedAccountUsedInTransaction && !SelectedAccountModel.IsFolderOfClosed;
        public bool IsEnabledDeleteAccount => !SelectedAccountModel.IsFolder && !_isSelectedAccountUsedInTransaction;


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