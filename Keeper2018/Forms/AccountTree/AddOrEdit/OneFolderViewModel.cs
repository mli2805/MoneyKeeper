using Caliburn.Micro;

namespace Keeper2018
{
    public class OneFolderViewModel : Screen
    {
        private bool _isInAddMode;
        private string _oldName;
        public AccountItemModel AccountItemInWork { get; set; }
        public string ParentFolder { get; set; }
        public bool IsSavePressed { get; set; }
      
        public void Initialize(AccountItemModel accountInWork, bool isInAddMode)
        {
            IsSavePressed = false;
            AccountItemInWork = accountInWork;
            _isInAddMode = isInAddMode;
            ParentFolder = AccountItemInWork.Parent == null ? "Корневой счет" : AccountItemInWork.Parent.Name;
            _oldName = accountInWork.Name;
        }

        protected override void OnViewLoaded(object view)
        {
            var cap = _isInAddMode ? "Добавить папку" : "Изменить название папки";
            DisplayName = $"{cap} (id = {AccountItemInWork.Id})";
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
                AccountItemInWork.Name = _oldName;
            }
            TryClose();
        }
    }
}
