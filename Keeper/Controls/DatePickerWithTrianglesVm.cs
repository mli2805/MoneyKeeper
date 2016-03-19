﻿using System;
using Caliburn.Micro;

namespace Keeper.Controls
{
    class DatePickerWithTrianglesVm : PropertyChangedBase
    {
        private DateTime _selectedDate;

        public DateTime SelectedDate
        {
            get { return _selectedDate; }
            set
            {
                if (value.Equals(_selectedDate)) return;
                _selectedDate = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
