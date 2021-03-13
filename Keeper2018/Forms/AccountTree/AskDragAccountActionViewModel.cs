using Caliburn.Micro;

namespace Keeper2018
{
    public enum DragAndDropAction { Before, Inside, After, Cancel }
    public class AskDragAccountActionViewModel : Screen
    {
        private readonly KeeperDataModel _keeperDataModel;
        public string Account1 { get; set; }
        public string Account2 { get; set; }
        public bool IsInsideEnabled {get; set;}

        public DragAndDropAction Answer { get; set; }

        public AskDragAccountActionViewModel(KeeperDataModel keeperDataModel)
        {
            _keeperDataModel = keeperDataModel;
        }

        public void Init(string account1, string account2)
        {
            Account1 = account1;
            Account2 = account2;
            IsInsideEnabled = !_keeperDataModel.AccountUsedInTransaction(_keeperDataModel.AccountByTitle(account2).Id);
            Answer = DragAndDropAction.Cancel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Question";
        }

        public void Before()
        {
            Answer = DragAndDropAction.Before;
            TryClose();
        }

        public void Inside()
        {
            Answer = DragAndDropAction.Inside;
            TryClose();
        }

        public void After()
        {
            Answer = DragAndDropAction.After;
            TryClose();
        }

        public void Cancel()
        {
            Answer = DragAndDropAction.Cancel;
            TryClose();
        }

    }
}
