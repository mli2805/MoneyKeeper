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

    public List<Transaction> TestList { get; set; }

    public TransactionsViewModel()
    {

      Db.Transactions.Load();
      Rows = Db.Transactions.Local;

      TestList = new List<Transaction>();
      for (int i=0; i<10; i++)
      {
        var tr = new Transaction();
        tr.Comment = "test" + i;
        TestList.Add(tr);
      }

    }
  }
}
