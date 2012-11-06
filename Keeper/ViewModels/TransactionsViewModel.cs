using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using Caliburn.Micro;
using Keeper.DomainModel;
using System.Linq;

namespace Keeper.ViewModels
{
  public static class AllOperationTypes
  {
    public static List<OperationType> OperationTypeList { get; set; }

    static AllOperationTypes()
    {
      OperationTypeList = Enum.GetValues(typeof (OperationType)).OfType<OperationType>().ToList();
    }
  }

  [Export(typeof(IShell))]
  [Export(typeof(ShellViewModel)), PartCreationPolicy(CreationPolicy.Shared)]
  public class TransactionsViewModel : Screen
  {
    public KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public ObservableCollection<Transaction> Rows { get; set; }
    public Transaction SelectedTransaction { get; set; }
    
    public TransactionsViewModel()
    {
      Db.Transactions.Load();
      Rows = Db.Transactions.Local;

    }

    public OperationType SelectedOperationType { get; set; }

  }


}
