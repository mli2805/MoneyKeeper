using System;
using System.ComponentModel;
using Caliburn.Micro;

namespace Keeper2018
{
    public class TransactionsViewModel : Screen
    {
        private readonly FilterModel _filterModel;
        private readonly FilterViewModel _filterViewModel;
        private readonly TranEditExecutor _tranEditExecutor;
        private readonly TranMoveExecutor _tranMoveExecutor;
        private readonly TranSelectExecutor _tranSelectExecutor;
        private readonly ComboTreesProvider _comboTreesProvider;

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
        public bool IsFirstLaunch = true;


        public TransactionsViewModel(TransModel model, FilterModel filterModel, FilterViewModel filterViewModel,
            TranEditExecutor tranEditExecutor, TranMoveExecutor tranMoveExecutor, TranSelectExecutor tranSelectExecutor,
            ComboTreesProvider comboTreesProvider)
        {
            Model = model;
            _filterModel = filterModel;
            _filterViewModel = filterViewModel;
            _tranEditExecutor = tranEditExecutor;
            _tranMoveExecutor = tranMoveExecutor;
            _tranSelectExecutor = tranSelectExecutor;
            _comboTreesProvider = comboTreesProvider;
            Top = 100;
            Left = 400;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Проводки";
        }

        public void Initialize()
        {
            _comboTreesProvider.Initialize();
            _filterModel.Initialize();
            Model.Initialize();
            IsFirstLaunch = false;
        }

        public void ButtonFilter()
        {
            var wm = new WindowManager();

            _filterViewModel.PlaceIt(Left, Top, FilterViewWidth);
            _filterViewModel.PropertyChanged += FilterViewModel_PropertyChanged;
            _filterViewModel.FilterModel.PropertyChanged += FilterModel_PropertyChanged;

            wm.ShowWindow(_filterViewModel);
        }

        private void FilterModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Model.SelectedTranWrappedForDatagrid != null)
                Model.SelectedTranWrappedForDatagrid.IsSelected = false;

            Model.SortedRows.Refresh();
            Model.SortedRows.MoveCurrentToLast();
            Model.SelectedTranWrappedForDatagrid = (TranWrappedForDatagrid) Model.SortedRows.CurrentItem;
        }

        private void FilterViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsActive" && _filterViewModel.IsActive == false)
            {
                Model.SortedRows.MoveCurrentToLast();
                Model.SelectedTranWrappedForDatagrid = (TranWrappedForDatagrid) Model.SortedRows.CurrentItem;
            }
        }

        public void ActionsMethod(TranAction action)
        {
            switch (action)
            {
                case TranAction.Edit:
                    _tranEditExecutor.EditSelected();
                    return;
                case TranAction.MoveUp:
                    _tranMoveExecutor.MoveSelected(TranMoveExecutor.Destination.Up);
                    return;
                case TranAction.MoveDown:
                    _tranMoveExecutor.MoveSelected(TranMoveExecutor.Destination.Down);
                    return;
                case TranAction.AddAfterSelected:
                    _tranEditExecutor.AddAfterSelected();
                    return;
                case TranAction.Delete:
                    _tranEditExecutor.DeleteSelected();
                    return;

                case TranAction.GoToDate:
                    _tranSelectExecutor.SelectFirstOfDate();
                    return;
                case TranAction.GoToEnd:
                    _tranSelectExecutor.SelectLast();
                    return;
                case TranAction.Filter: return;
            }
        }

        public override void CanClose(Action<bool> callback)
        {
            _filterViewModel.TryClose();
            base.CanClose(callback);
        }
    }
}