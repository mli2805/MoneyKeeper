﻿using System.Windows;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class DurationModel : PropertyChangedBase
    {
        private bool _isPerpetual;
        public bool IsPerpetual
        {
            get => _isPerpetual;
            set
            {
                if (value == _isPerpetual) return;
                _isPerpetual = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(ValueVisibility));
            }
        } // term-less

        public int Value { get; set; }
        public Durations Scale { get; set; }

        public Visibility ValueVisibility => IsPerpetual ? Visibility.Hidden : Visibility.Visible;
       

        public DurationModel()
        {
            IsPerpetual = true;
        }

        public DurationModel(int value, Durations scale)
        {
            Value = value;
            Scale = scale;
            IsPerpetual = false;
        }

        public override string ToString()
        {
            if (IsPerpetual) return "не ограничен";
            switch (Scale)
            {
                case Durations.Years: return $"{Value} {Value.YearsNumber()}";
                case Durations.Months: return $"{Value} {Value.MonthsNumber()}";
                case Durations.Days: return $"{Value} {Value.DaysNumber()}";

                default: return base.ToString();
            }
        }

        public DurationModel Clone()
        {
            return (DurationModel)MemberwiseClone();
        }
    }
}