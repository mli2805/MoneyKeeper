using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Composition;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Transactions;
using Keeper.Utils;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class TransViewModel : Screen
    {

        public TransModel Model { get; set; } = new TransModel();
        public TranActions ActionsHandler { get; set; } = new TranActions();

        [ImportingConstructor]
        public TransViewModel(KeeperDb db)
        {
            Model.Db = db;
            IoC.Get<TransactionsConvertor>().Convert();
            Model.Rows = WrapTransactions(Model.Db.TransWithTags);
            Model.SortedRows = CollectionViewSource.GetDefaultView(Model.Rows);
            Model.SortedRows.SortDescriptions.Add(new SortDescription("Tran.Timestamp", ListSortDirection.Ascending));

            Model.SelectedTranWrappedForDatagrid = Model.Rows.OrderBy(t => t.Tran.Timestamp).Last();
            Model.SelectedTranWrappedForDatagrid.IsSelected = true;
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
        public void ActionsMethod(int code)
        {
            ActionsHandler.Do(code, Model);
        }
    }
}
