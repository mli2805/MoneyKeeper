using System.Collections.Generic;
using Caliburn.Micro;
using Keeper.DomainModel.WorkTypes;

namespace Keeper.Controls.OneTranViewControls.SubControls.AccNameSelectionControl
{
    class AccNameSelectorVm : PropertyChangedBase
    {
        public List<AccNameButtonVm> Buttons { get; set; }

        private AccName _myAccName;
        private List<AccName> _availableAccNames;

        public AccName MyAccName
        {
            get { return _myAccName; }
            set
            {
                if (Equals(value, _myAccName)) return;
                _myAccName = value;
                NotifyOfPropertyChange();
            }
        }

        public List<AccName> AvailableAccNames
        {
            get { return _availableAccNames; }
            set
            {
                if (Equals(value, _availableAccNames)) return;
                _availableAccNames = value;
                NotifyOfPropertyChange();
            }
        }

        public string ControlTitle { get; set; }

    }
}
