using System;
using System.ComponentModel;
using Caliburn.Micro;

namespace Keeper2018
{
    public class TransactsViewModel : Screen
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


        private readonly ComboTreesProvider _comboTreesProvider;
        private readonly TransactsEditExecutor _transactsEditExecutor;
        private readonly TransactsMoveExecutor _transactsMoveExecutor;
        private readonly TransactsGotoExecutor _transactsGotoExecutor;
        private readonly FilterModel _filterModel;
        private readonly FilterViewModel _filterViewModel;
      
        public TransactsModel Model { get; set; }
        public TransactsViewModel(TransactsModel model, ComboTreesProvider comboTreesProvider,
            TransactsEditExecutor transactsEditExecutor, TransactsMoveExecutor transactsMoveExecutor, TransactsGotoExecutor transactsGotoExecutor,
            FilterModel filterModel, FilterViewModel filterViewModel)
        {
            Model = model;
            _comboTreesProvider = comboTreesProvider;
            _transactsEditExecutor = transactsEditExecutor;
            _transactsMoveExecutor = transactsMoveExecutor;
            _transactsGotoExecutor = transactsGotoExecutor;
            _filterModel = filterModel;
            _filterViewModel = filterViewModel;

            Top = 100;
            Left = 400;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Проводки";
            Model.IsCollectionChanged = false;
        }

        public void Initialize()
        {
            _comboTreesProvider.Initialize();
            _filterModel.Initialize();
           
            Model.Initialize();
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
            if (Model.SelectedTranWrappedForDataGrid != null)
                Model.SelectedTranWrappedForDataGrid.IsSelected = false;

            Model.SortedRows.Refresh();
            Model.SortedRows.MoveCurrentToLast();
            Model.SelectedTranWrappedForDataGrid = (TranWrappedForDataGrid)Model.SortedRows.CurrentItem;
        }

        private void FilterViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsActive" && _filterViewModel.IsActive == false)
            {
                Model.SortedRows.MoveCurrentToLast();
                Model.SelectedTranWrappedForDataGrid = (TranWrappedForDataGrid)Model.SortedRows.CurrentItem;
            }
        }

        public void Calculator()
        {
            System.Diagnostics.Process.Start("calc");
        }

        public void ActionsMethod(TranAction action)
        {
            switch (action)
            {
                case TranAction.Calculator:
                    Calculator();
                    return;
                case TranAction.Filter:
                    ButtonFilter();
                    return;
                case TranAction.Edit:
                    _transactsEditExecutor.EditSelected();
                    return;
                case TranAction.MoveUp:
                    _transactsMoveExecutor.MoveSelected(TransactsMoveExecutor.Destination.Up);
                    return;
                case TranAction.MoveDown:
                    (_transactsMoveExecutor).MoveSelected(TransactsMoveExecutor.Destination.Down);
                    return;
                case TranAction.AddAfterSelected:
                    _transactsEditExecutor.AddAfterSelected();
                    return;
                case TranAction.Delete:
                    _transactsEditExecutor.DeleteSelected();
                    return;

                case TranAction.GoToDate:
                    _transactsGotoExecutor.SelectFirstOfDate();
                    return;
                case TranAction.GoToEnd:
                    _transactsGotoExecutor.SelectLast();
                    return;
            }
        }

        public override void CanClose(Action<bool> callback)
        {
            _filterViewModel.TryClose();
            base.CanClose(callback);
        }
    }
}
