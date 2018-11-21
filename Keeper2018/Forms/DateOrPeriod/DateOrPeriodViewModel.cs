using Caliburn.Micro;

namespace Keeper2018
{
    public class DateOrPeriodViewModel : Screen
    {
        private readonly ShellPartsBinder _shellPartsBinder;

        private string _buttonContent;
        public string ButtonContent
        {
            get => _buttonContent;
            set
            {
                if (value == _buttonContent) return;
                _buttonContent = value;
                NotifyOfPropertyChange();
            }
        }

        public DateOrPeriodViewModel(ShellPartsBinder shellPartsBinder)
        {
            _shellPartsBinder = shellPartsBinder;
            _shellPartsBinder.PropertyChanged += _shellPartsBinder_PropertyChanged;
        }

        private void _shellPartsBinder_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ButtonContent = _shellPartsBinder.BalanceOrTraffic == BalanceOrTraffic.Balance
                ? "Показать обороты за"
                : "Показать остатки по: (включительно)";
        }

        public void ChangeMode()
        {
            _shellPartsBinder.BalanceOrTraffic = _shellPartsBinder.BalanceOrTraffic == BalanceOrTraffic.Balance 
                ? BalanceOrTraffic.Traffic 
                : BalanceOrTraffic.Balance;
        }

    }
}
