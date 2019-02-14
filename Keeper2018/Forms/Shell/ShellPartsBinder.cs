using System;
using System.Windows;
using Caliburn.Micro;

namespace Keeper2018
{
    public class ShellPartsBinder : PropertyChangedBase
    {
        private AccountModel _selectedAccountModel;

        public AccountModel SelectedAccountModel
        {
            get => _selectedAccountModel;
            set
            {
                if (Equals(value, _selectedAccountModel)) return;
                _selectedAccountModel = value;
                NotifyOfPropertyChange();
            }
        }

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
    }
}