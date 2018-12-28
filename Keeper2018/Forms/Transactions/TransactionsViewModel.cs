using System.ComponentModel;
using Caliburn.Micro;

namespace Keeper2018
{
    public class TransactionsViewModel : Screen
    {
        private readonly FilterModel _filterModel;
        private readonly TranEditActionsExecutor _tranEditActionsExecutor;
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
        private FilterViewModel _filterViewModel;
        public TransModel Model { get; set; }


        public TranLocateActionsExecutor LocateActionsExecutorHandler { get; set; } = new TranLocateActionsExecutor();

        public TransactionsViewModel(TransModel model, FilterModel filterModel, FilterViewModel filterViewModel,
            TranEditActionsExecutor tranEditActionsExecutor, ComboTreesProvider comboTreesProvider)
        {
            Model = model;
            _filterModel = filterModel;
            _filterViewModel = filterViewModel;
            _tranEditActionsExecutor = tranEditActionsExecutor;
            _comboTreesProvider = comboTreesProvider;
            Top = 100;
            Left = 700;
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
            Model.SortedRows.Refresh();
            Model.SortedRows.MoveCurrentToLast();
            Model.SelectedTranWrappedForDatagrid = (TranWrappedForDatagrid)Model.SortedRows.CurrentItem;
        }

        private void FilterViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsActive" && _filterViewModel.IsActive == false)
            {
//                Model.FilterModel = null;

                Model.SortedRows.MoveCurrentToLast();
                Model.SelectedTranWrappedForDatagrid = (TranWrappedForDatagrid)Model.SortedRows.CurrentItem;
            }
        }

        public void ActionsMethod(TranAction action)
        {
            if ((int)action < 11)
            {
                if (_tranEditActionsExecutor.Do(action, Model))
                    Model.IsCollectionChanged = true;
            }
            else
            {
                LocateActionsExecutorHandler.Do(action, Model);
            }
        }
    }
}
