using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Keeper.Annotations;

namespace Keeper.Controls
{
    public class PeriodChoiceControlModel : INotifyPropertyChanged
    {
        public const int MinCenterPartWidth = 30;
        public bool _btnFromIsHolded;
        public bool _btnToIsHolded;
        public double _btnFromStartX;
        public double _btnToStartX;
        public bool _centralPartDoubleClick;
        public bool _centralPartIsHolded;
        public double _centralPartStartX;

        private Thickness _btnFromMargin;
        private Thickness _btnToMargin;
        private double _leftPartWidth;
        private double _centerPartWidth;
        private Thickness _centerPartMargin;
        private Thickness _rightPartMargin;
        public Thickness BtnFromMargin
        {
            get { return _btnFromMargin; }
            set
            {
                if (value.Equals(_btnFromMargin)) return;
                _btnFromMargin = value;
                OnPropertyChanged();
            }
        }
        public Thickness BtnToMargin
        {
            get { return _btnToMargin; }
            set
            {
                if (value.Equals(_btnToMargin)) return;
                double delta = _btnToMargin.Left - value.Left;
                _btnToMargin = value;
                RightPartMargin = new Thickness(RightPartMargin.Left - delta, 0, 0, 0);
                OnPropertyChanged();
            }
        }
        public double LeftPartWidth
        {
            get { return _leftPartWidth; }
            set
            {
                if (value.Equals(_leftPartWidth)) return;
                _leftPartWidth = value;
                OnPropertyChanged();
            }
        }
        public double CenterPartWidth
        {
            get { return _centerPartWidth; }
            set
            {
                if (value.Equals(_centerPartWidth)) return;
                _centerPartWidth = value;
                OnPropertyChanged();
            }
        }
        public Thickness CenterPartMargin
        {
            get { return _centerPartMargin; }
            set
            {
                if (value.Equals(_centerPartMargin)) return;
                _centerPartMargin = value;
                OnPropertyChanged();
            }
        }
        public Thickness RightPartMargin
        {
            get { return _rightPartMargin; }
            set
            {
                if (value.Equals(_rightPartMargin)) return;
                _rightPartMargin = value;
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
