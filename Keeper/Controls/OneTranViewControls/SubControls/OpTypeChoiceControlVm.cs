using Caliburn.Micro;
using Keeper.DomainModel.Enumes;

namespace Keeper.Controls.OneTranViewControls.SubControls
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
