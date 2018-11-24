using System;
using System.Windows;
using Caliburn.Micro;

namespace Keeper2018
{
    public class TwoSelectorsViewModel : Screen
    {
        public ShellPartsBinder ShellPartsBinder { get; set; }

        private bool _isPeriod;
        public bool IsPeriod
        {
            get => _isPeriod;
            set
            {
                if (value == _isPeriod) return;
                _isPeriod = value;
                NotifyOfPropertyChange();
                Switch();
            }
        }

        private Visibility _periodSelectControlVisibility;
        public Visibility PeriodSelectControlVisibility
        {
            get => _periodSelectControlVisibility;
            set
            {
                if (Equals(value, _periodSelectControlVisibility)) return;
                _periodSelectControlVisibility = value;
                NotifyOfPropertyChange(() => PeriodSelectControlVisibility);
            }
        }

        private Visibility _dateSelectControlVisibility;
        public Visibility DateSelectControlVisibility
        {
            get => _dateSelectControlVisibility;
            set
            {
                if (Equals(value, _dateSelectControlVisibility)) return;
                _dateSelectControlVisibility = value;
                NotifyOfPropertyChange(() => DateSelectControlVisibility);
            }
        }

      
        private void Switch()
        {
            if (_isPeriod)
            {
                ShellPartsBinder.BalanceOrTraffic = BalanceOrTraffic.Traffic;
                DateSelectControlVisibility = Visibility.Collapsed;
                PeriodSelectControlVisibility = Visibility.Visible;
            }
            else
            {
                ShellPartsBinder.BalanceOrTraffic = BalanceOrTraffic.Balance;
                DateSelectControlVisibility = Visibility.Visible;
                PeriodSelectControlVisibility = Visibility.Collapsed;
            }
        }

        public TwoSelectorsViewModel(ShellPartsBinder shellPartsBinder)
        {
            ShellPartsBinder = shellPartsBinder;

            Switch();
        }
    }
}
