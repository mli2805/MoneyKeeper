using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  public class TransactionsViewModel : Screen
  {
    public KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public ObservableCollection<Transaction> Rows { get; set; }
    
    public TransactionsViewModel()
    {
      Db.Transactions.Load();
      Rows = Db.Transactions.Local;
    }
  }
}
