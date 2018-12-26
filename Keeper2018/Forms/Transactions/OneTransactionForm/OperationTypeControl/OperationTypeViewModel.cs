using Caliburn.Micro;

namespace Keeper2018
{
    public class OperationTypeViewModel : PropertyChangedBase
    {
        private OperationType _selectedOperationType;

        public OperationType SelectedOperationType
        {
            get { return _selectedOperationType; }
            set
            {
                if (value == _selectedOperationType) return;
                _selectedOperationType = value;
                NotifyOfPropertyChange();
            }
        }
      
        public void B0()
        {
            SelectedOperationType = OperationType.Доход;
        }
        public void B1()
        {
            SelectedOperationType = OperationType.Расход;
        }
        public void B2()
        {
            SelectedOperationType = OperationType.Перенос;
        }
        public void B3()
        {
            SelectedOperationType = OperationType.Обмен;
        }
    }
}
