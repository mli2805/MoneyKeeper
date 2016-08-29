﻿using System.Collections.ObjectModel;
using Caliburn.Micro;
using Keeper.Controls.OneTranViewControls.SubControls.AccNameSelectionControl;
using Keeper.DomainModel.WorkTypes;

namespace Keeper.Controls.OneTranViewControls.SubControls.TagPickingControl
{
    class TagPickerVm : PropertyChangedBase
    {
        public ObservableCollection<AccName> Tags { get; set; } = new ObservableCollection<AccName>();
        public AccNameSelectorVm TagSelectorVm { get; set; }
    }
}
