using Caliburn.Micro;

namespace Keeper2018
{
    public class OneAccountViewModel : Screen
    {
        private bool _isInAddMode;
        private string _oldName;
        public AccountModel AccountInWork { get; set; }
        public string ParentFolder { get; set; }

        public bool IsSavePressed { get; set; }

        public void Initialize(AccountModel accountInWork, bool isInAddMode)
        {
            IsSavePressed = false;
            AccountInWork = accountInWork;
            _isInAddMode = isInAddMode;

            ParentFolder = AccountInWork.Owner.Name;
            _oldName = accountInWork.Name;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _isInAddMode ? "Добавить" : "Изменить";
        }

        public void Save()
        {
            IsSavePressed = true;
            TryClose();
        }

        public void Cancel()
        {
            if (!_isInAddMode)
            {
                AccountInWork.Header = _oldName;
            }
            TryClose();
        }
    }
}
