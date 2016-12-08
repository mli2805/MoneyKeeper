using System;
using System.ComponentModel;
using System.Composition;
using Caliburn.Micro;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Trans;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class TransViewModel : Screen
    {
        private int _left;
        public int Left
        {
            get { return _left; }
            set
            {
                _left = value;
                _filterViewModel?.PlaceIt(Left, Top, FilterViewWidth);
            }
        }
        public int Top { get; set; }
        public int FilterViewWidth = 225;
        public TransModel Model { get; set; }
        public TranEditActionsExecutor EditActionsExecutorHandler { get; set; } = new TranEditActionsExecutor();
        public TranLocateActionsExecutor LocateActionsExecutorHandler { get; set; } = new TranLocateActionsExecutor();
        public bool IsCollectionChanged => Model.IsCollectionChanged;

        private FilterViewModel _filterViewModel;

        [ImportingConstructor]
        public TransViewModel(KeeperDb db)
        {
            Model = new TransModel(db);
            Top = 100;
            Left = 700;

        }
        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Transactions with tags";
        }
        public override void CanClose(Action<bool> callback)
        {
            if (_filterViewModel != null && _filterViewModel.IsActive)
                _filterViewModel.TryClose();
            base.CanClose(callback);
        }
        public void ButtonFilter()
        {
            var wm = new WindowManager();

            _filterViewModel = IoC.Get<FilterViewModel>();
            _filterViewModel.PlaceIt(Left, Top, FilterViewWidth);
            _filterViewModel.PropertyChanged += FilterViewModel_PropertyChanged;
            _filterViewModel.FilterModel.PropertyChanged += FilterModel_PropertyChanged;

            wm.ShowWindow(_filterViewModel);
        }

        private void FilterModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Model.FilterModel = _filterViewModel.FilterModel;

            Model.SortedRows.MoveCurrentToLast();
            Model.SelectedTranWrappedForDatagrid = (TranWrappedForDatagrid)Model.SortedRows.CurrentItem;
        }

        private void FilterViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsActive" && _filterViewModel.IsActive == false)
            {
                Model.FilterModel = null;

                Model.SortedRows.MoveCurrentToLast();
                Model.SelectedTranWrappedForDatagrid = (TranWrappedForDatagrid)Model.SortedRows.CurrentItem;
            }
        }

        public void ActionsMethod(TranAction action)
        {
            if ((int) action < 11)
            {
                if (EditActionsExecutorHandler.Do(action, Model))
                    Model.IsCollectionChanged = true;
            }
            else
            {
                LocateActionsExecutorHandler.Do(action, Model);
            }
        }
    }
}
