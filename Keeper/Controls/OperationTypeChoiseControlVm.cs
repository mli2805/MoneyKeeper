using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Keeper.DomainModel.Enumes;

namespace Keeper.Controls
{
    class OperationTypeChoiseControlVm : PropertyChangedBase
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

        private void B0() { PressedButton = OperationType.Доход; }
        private void B1() { PressedButton = OperationType.Расход; }
        private void B2() { PressedButton = OperationType.Перенос; }
        private void B3() { PressedButton = OperationType.Обмен; }
        private void B4() { PressedButton = OperationType.ОбменПеренос; }
    }
}
