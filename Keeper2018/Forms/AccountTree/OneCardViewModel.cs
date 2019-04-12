using System.Globalization;
using Caliburn.Micro;

namespace Keeper2018
{
    public class OneCardViewModel : Screen
    {
        private bool _isInAddMode;
        private AccountModel _accountModel;
        public PayCard CardInWork { get; set; }
        public string ParentName { get; set; }

        private string _junction;
        public string Junction
        {
            get => _junction;
            set
            {
                if (value == _junction) return;
                _junction = value;
                NotifyOfPropertyChange();
            }
        }
        public bool IsSavePressed { get; set; }

        public void InitializeForm(AccountModel accountModel, bool isInAddMode)
        {
            IsSavePressed = false;
            _isInAddMode = isInAddMode;
            _accountModel = accountModel;
            ParentName = accountModel.Owner.Name;

            CardInWork = accountModel.PayCard;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _isInAddMode ? "Добавить" : "Изменить";
        }

        public void SaveCard()
        {
            IsSavePressed = true;
            if (_isInAddMode)
                _accountModel.Header = CardInWork.Name;
            TryClose();
        }

        public void Cancel()
        {
            IsSavePressed = false;
            TryClose();
        }
        public void CompileAccountName()
        {
           
            var startDate = CardInWork.StartDate.ToString("d/MM/yyyy", CultureInfo.InvariantCulture);
            var finishDate = CardInWork.FinishDate.ToString("d/MM/yyyy", CultureInfo.InvariantCulture);
            Junction = $"{CardInWork.MainCurrency} {startDate} - {finishDate}";
        }

    }
}
