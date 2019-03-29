﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Keeper2018.Properties;

namespace Keeper2018
{
    public sealed class PeriodChoiceControlModel : INotifyPropertyChanged
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
        private Thickness _btnToMargin;
        private double _leftPartWidth;
        private double _centerPartWidth;
        private Thickness _centerPartMargin;
        private Thickness _rightPartMargin;
        public Thickness BtnFromMargin
        {
            get => _btnFromMargin;
            set
            {
                if (value.Equals(_btnFromMargin)) return;
                _btnFromMargin = value;
                OnPropertyChanged();
            }
        }
        public Thickness BtnToMargin
        {
            get => _btnToMargin;
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
            get => _leftPartWidth;
            set
            {
                if (value.Equals(_leftPartWidth)) return;
                _leftPartWidth = value;
                OnPropertyChanged();
            }
        }
        public double CenterPartWidth
        {
            get => _centerPartWidth;
            set
            {
                if (value.Equals(_centerPartWidth)) return;
                _centerPartWidth = value;
                OnPropertyChanged();
            }
        }
        public Thickness CenterPartMargin
        {
            get => _centerPartMargin;
            set
            {
                if (value.Equals(_centerPartMargin)) return;
                _centerPartMargin = value;
                OnPropertyChanged();
            }
        }
        public Thickness RightPartMargin
        {
            get => _rightPartMargin;
            set
            {
                if (value.Equals(_rightPartMargin)) return;
                _rightPartMargin = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
