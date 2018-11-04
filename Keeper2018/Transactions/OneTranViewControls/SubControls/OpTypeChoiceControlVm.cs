using Caliburn.Micro;

namespace Keeper2018
{
    class OpTypeChoiceControlVm : PropertyChangedBase
    {
        private OperationType _pressedButton;

        public OperationType PressedButton
        {
            get { return _pressedButton; }
            set
            {
                if (value == _pressedButton) return;
                _pressedButton = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
