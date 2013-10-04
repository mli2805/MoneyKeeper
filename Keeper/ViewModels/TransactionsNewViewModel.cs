using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  [Export, PartCreationPolicy(CreationPolicy.Shared)] // для того чтобы в классе Transaction можно было обратиться к здешнему свойству SelectedTransaction
  class TransactionsNewViewModel : Screen
  {
    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }
    public static IWindowManager WindowManager { get { return IoC.Get<IWindowManager>(); } }
    public ObservableCollection<Transaction> Rows { get; set; }
    public ICollectionView SortedRows { get; set; }

    public TransactionsNewViewModel()
    {
      Rows = Db.Transactions;

    }
  }
}
