using System.ComponentModel;
using System.Runtime.CompilerServices;
using Keeper.Annotations;

namespace Keeper.Utils.OxyPlots
{
    public class RatesDiagramContentModel : INotifyPropertyChanged
    {
        private bool _isCheckedUsdNbRb;
        private bool _isCheckedMyUsd;
        private bool _isCheckedEurNbRb;
        private bool _isCheckedEurUsdNbRb;
        private bool _isCheckedRurNbRb;
        private bool _isCheckedRurUsd;
        private bool _isCheckedBusketNbRb;
        private bool _isCheckedLogarithm;
        private bool _isCheckedUnify;

        public bool IsCheckedUsdNbRb
        {
            get { return _isCheckedUsdNbRb; }
            set
            {
                if (value.Equals(_isCheckedUsdNbRb)) return;
                _isCheckedUsdNbRb = value;
                OnPropertyChanged();
            }
        }
        public bool IsCheckedMyUsd
        {
            get { return _isCheckedMyUsd; }
            set
            {
                if (value.Equals(_isCheckedMyUsd)) return;
                _isCheckedMyUsd = value;
                OnPropertyChanged();
            }
        }
        public bool IsCheckedEurNbRb
        {
            get { return _isCheckedEurNbRb; }
            set
            {
                if (value.Equals(_isCheckedEurNbRb)) return;
                _isCheckedEurNbRb = value;
                OnPropertyChanged();
            }
        }
        public bool IsCheckedEurUsdNbRb
        {
            get { return _isCheckedEurUsdNbRb; }
            set
            {
                if (value.Equals(_isCheckedEurUsdNbRb)) return;
                _isCheckedEurUsdNbRb = value;
                OnPropertyChanged();
            }
        }
        public bool IsCheckedRurNbRb
        {
            get { return _isCheckedRurNbRb; }
            set
            {
                if (value.Equals(_isCheckedRurNbRb)) return;
                _isCheckedRurNbRb = value;
                OnPropertyChanged();
            }
        }
        public bool IsCheckedRurUsd
        {
            get { return _isCheckedRurUsd; }
            set
            {
                if (value.Equals(_isCheckedRurUsd)) return;
                _isCheckedRurUsd = value;
                OnPropertyChanged();
            }
        }
        public bool IsCheckedBusketNbRb
        {
            get { return _isCheckedBusketNbRb; }
            set
            {
                if (value.Equals(_isCheckedBusketNbRb)) return;
                _isCheckedBusketNbRb = value;
                OnPropertyChanged();
            }
        }
        public bool IsCheckedLogarithm
        {
            get { return _isCheckedLogarithm; }
            set
            {
                if (value.Equals(_isCheckedLogarithm)) return;
                _isCheckedLogarithm = value;
                OnPropertyChanged();
            }
        }
        public bool IsCheckedUnify
        {
            get { return _isCheckedUnify; }
            set
            {
                if (value.Equals(_isCheckedUnify)) return;
                _isCheckedUnify = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
