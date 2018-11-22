using System;
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

        public Period SelectedPeriod { get; set; } = new Period(){FinishMoment = DateTime.Today.Date.AddDays(1).AddSeconds(-1)};
    }
}