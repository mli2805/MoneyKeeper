using System.Windows;
using Caliburn.Micro;

namespace Keeper2018
{
    public sealed class PeriodChoiceControlModel : PropertyChangedBase
    {
        public const int MinCenterPartWidth = 45;

        public bool BtnFromIsHolded;
        public bool BtnToIsHolded;
        public double BtnFromStartX;
        public double BtnToStartX;
        public bool CentralPartDoubleClick;
        public bool CentralPartIsHolded;
        public double CentralPartStartX;

        private Thickness _btnFromMargin;
        public Thickness BtnFromMargin
        {
            get => _btnFromMargin;
            set
            {
                if (value.Equals(_btnFromMargin)) return;
                _btnFromMargin = value;
                NotifyOfPropertyChange();
            }
        }

        private Thickness _btnToMargin;
        public Thickness BtnToMargin
        {
            get => _btnToMargin;
            set
            {
                if (value.Equals(_btnToMargin)) return;
                double delta = _btnToMargin.Left - value.Left;
                _btnToMargin = value;
                RightPartMargin = new Thickness(RightPartMargin.Left - delta, 0, 0, 0);
                NotifyOfPropertyChange();
            }
        }
      
        private double _leftPartWidth;
        public double LeftPartWidth
        {
            get => _leftPartWidth;
            set
            {
                if (value.Equals(_leftPartWidth)) return;
                _leftPartWidth = value;
                NotifyOfPropertyChange();
            }
        }
   
        private double _centerPartWidth;
        public double CenterPartWidth
        {
            get => _centerPartWidth;
            set
            {
                if (value.Equals(_centerPartWidth)) return;
                _centerPartWidth = value;
                NotifyOfPropertyChange();
            }
        }

        private Thickness _centerPartMargin;
        public Thickness CenterPartMargin
        {
            get => _centerPartMargin;
            set
            {
                if (value.Equals(_centerPartMargin)) return;
                _centerPartMargin = value;
                NotifyOfPropertyChange();
            }
        }
      
        private Thickness _rightPartMargin;
        public Thickness RightPartMargin
        {
            get => _rightPartMargin;
            set
            {
                if (value.Equals(_rightPartMargin)) return;
                _rightPartMargin = value;
                NotifyOfPropertyChange();
            }
        }

       

    }
}
