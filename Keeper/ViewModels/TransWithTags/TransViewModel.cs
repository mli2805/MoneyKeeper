using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Composition;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Trans;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class TransViewModel : Screen
    {

        public TransModel Model { get; set; } = new TransModel();
        public TranEditActionsExecutor EditActionsExecutorHandler { get; set; } = new TranEditActionsExecutor();
        public TranLocateActionsExecutor LocateActionsExecutorHandler { get; set; } = new TranLocateActionsExecutor();
        public bool IsCollectionChanged { get; set; }

        [ImportingConstructor]
        public TransViewModel(KeeperDb db)
        {
            Model.Db = db;

            Model.Rows = WrapTransactions(Model.Db.TransWithTags);
            Model.Rows.CollectionChanged += Rows_CollectionChanged;

            Model.SortedRows = CollectionViewSource.GetDefaultView(Model.Rows);
            Model.SortedRows.SortDescriptions.Add(new SortDescription("Tran.Timestamp", ListSortDirection.Ascending));

            Model.SelectedTranWrappedForDatagrid = Model.Rows.OrderBy(t => t.Tran.Timestamp).Last();
            Model.SelectedTranWrappedForDatagrid.IsSelected = true;

            IsCollectionChanged = false;
        }

        private void Rows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IsCollectionChanged = true;
        }

        private ObservableCollection<TranWrappedForDatagrid> WrapTransactions(ObservableCollection<TranWithTags> transactions)
        {
            var result = new ObservableCollection<TranWrappedForDatagrid>();
            foreach (var tran in transactions)
            {
                result.Add(new TranWrappedForDatagrid() { Tran = tran });
            }
            return result;
        }
        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Transactions with tags";
        }
        public void ButtonClose()
        {
            TryClose();
        }
        public void ActionsMethod(TranAction action)
        {
            if ((int) action < 11)
            {
                if (EditActionsExecutorHandler.Do(action, Model))
                    IsCollectionChanged = true;
            }
            else
            {
                LocateActionsExecutorHandler.Do(action, Model);
            }
        }
    }
}
