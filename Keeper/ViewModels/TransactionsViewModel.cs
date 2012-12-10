using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Windows.Controls;
using System.Windows.Data;
using Caliburn.Micro;
using Keeper.DomainModel;
using System.Linq;

namespace Keeper.ViewModels
{
  [Export(typeof(IShell))]
  [Export(typeof(TransactionsViewModel)), PartCreationPolicy(CreationPolicy.Shared)]
  public class TransactionsViewModel : Screen, IShell
  {
    #region // объявление и инициализация листов для комбиков
    public List<OperationType> OperationTypes { get; set; }
    public List<CurrencyCodes> CurrencyList { get; set; }

    public List<Account> MyAccounts { get; set; }
    public List<Account> MyAccountsForShopping { get; set; }

    public List<Account> AccountsWhoGivesMeMoney { get; set; }
    public List<Account> AccountsWhoTakesMyMoney { get; set; }
    public List<Account> BankAccounts { get; set; }
    public List<Account> IncomeArticles { get; set; }
    public List<Account> ExpenseArticles { get; set; }

    public void ComboBoxesValues()
    {
      OperationTypes = Enum.GetValues(typeof(OperationType)).OfType<OperationType>().ToList();
      CurrencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();

      MyAccounts = (Db.Accounts.Local.Where(account => account.GetRootName() == "Мои" &&
        account.Children.Count == 0)).ToList();
      MyAccountsForShopping = (Db.Accounts.Local.Where(account => account.GetRootName() == "Мои" &&
        account.Children.Count == 0 && !account.IsDescendantOf("Депозиты"))).ToList();
      AccountsWhoGivesMeMoney = (Db.Accounts.Local.Where(account => (account.IsDescendantOf("ДеньгоДатели") ||
        account.IsDescendantOf("Банки")) && account.Children.Count == 0)).ToList();
      AccountsWhoTakesMyMoney = (Db.Accounts.Local.Where(account => account.IsDescendantOf("ДеньгоПолучатели") &&
        account.Children.Count == 0)).ToList();
      BankAccounts = (Db.Accounts.Local.Where(account => account.IsDescendantOf("Банки") &&
        account.Children.Count == 0)).ToList();
      IncomeArticles = (Db.Accounts.Local.Where(account => account.GetRootName() == "Все доходы" &&
        account.Children.Count == 0)).ToList();
      ExpenseArticles = (Db.Accounts.Local.Where(account => account.GetRootName() == "Все расходы" &&
        account.Children.Count == 0)).ToList();
    }
    #endregion

    public KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public ObservableCollection<Transaction> Rows { get; set; }
    public ICollectionView SortedRows { get; set; }

    /// <summary>
    /// при смене SelectedTransaction может оказаться что новая SelectedTransaction имеет другой тип операции
    /// и нужно открыть другую закладку TabControlа
    /// тогда поднимаем этот флаг , чтобы отличить эту ситуацию от случая когда юзер меняет тип операции
    /// для текущей SelectedTransaction и надо очистить комбики
    /// </summary>
    private bool _isInTransactionSelectionProcess;
    private bool _isInAddTransactionMode;

    private bool _isTransactionInWorkChanged;
    public bool IsTransactionInWorkChanged
    {
      get { return _isTransactionInWorkChanged; }
      set
      {
        if (value.Equals(_isTransactionInWorkChanged)) return;
        if (_isInTransactionSelectionProcess) return;
        _isTransactionInWorkChanged = value;
        CanSaveTransactionChanges = value;
        CanCancelTransactionChanges = value;
        NotifyOfPropertyChange(() => CanSaveTransactionChanges);
        NotifyOfPropertyChange(() => CanCancelTransactionChanges);
      }
    }

    private Transaction _transactionInWork;
    public Transaction TransactionInWork
    {
      get { return _transactionInWork; }
      set
      {
        if (Equals(value, _transactionInWork)) return;
        _transactionInWork = value;
        NotifyOfPropertyChange(() => TransactionInWork);
      }
    }

    private int _selectedTabIndex;
    public int SelectedTabIndex
    {
      get { return _selectedTabIndex; }
      set
      {
        if (value == _selectedTabIndex) return;
        _selectedTabIndex = value;
        NotifyOfPropertyChange(() => SelectedTabIndex);
        _transactionInWork.Operation = (OperationType)_selectedTabIndex;
        if (!_isInTransactionSelectionProcess)
        {
          if (HaveTheSameOperationType(SelectedTransaction, TransactionInWork))
          {
            SelectedTransaction.Operation = TransactionInWork.Operation;
            TransactionInWork.Debet = SelectedTransaction.Debet;
            TransactionInWork.Credit = SelectedTransaction.Credit;
            TransactionInWork.Article = SelectedTransaction.Article;
          }
          else
          {
            SelectedTransaction.Operation = (OperationType)(900 + ExtractPureOperationType((int)SelectedTransaction.Operation));
            _transactionInWork.Debet = null;
            _transactionInWork.Credit = null;
            _transactionInWork.Article = null;
          }
        }
      }
    }

    private Transaction _selectedTransaction;
    public Transaction SelectedTransaction
    {
      get { return _selectedTransaction; }
      set
      {
        if (Equals(value, _selectedTransaction)) return;
        _selectedTransaction = value;
//        NotifyOfPropertyChange(() => SelectedTransaction);

        _isInTransactionSelectionProcess = true; // ставим флаг, чтобы если меняется тип операции в сеттере SelectedTabIndex не удалялись поля транзакции
        if (!_isInAddTransactionMode) _transactionInWork.CloneFrom(_selectedTransaction);// else TransactionInWork уже сформирован методом Preform
        SelectedTabIndex = (int)_transactionInWork.Operation;
        _isInTransactionSelectionProcess = false;
        NotifyOfPropertyChange(() => TransactionInWork);
        NotifyOfPropertyChange(() => AmountInUsd);
      }
    }

    public string AmountInUsd
    {
      get
      {
        if (TransactionInWork.Currency == CurrencyCodes.USD) return "";
        var rate = new CurrencyRate();

        rate =
          Db.CurrencyRates.Local.FirstOrDefault(
            currencyRate => ((currencyRate.BankDay.Date == TransactionInWork.Timestamp.Date) && (currencyRate.Currency == TransactionInWork.Currency)));

        if (rate == null) return "отсутствует курс на эту дату";
        var res = (TransactionInWork.Amount / (decimal)rate.Rate).ToString("F2") + "$ по курсу " + rate.Rate;
        if (TransactionInWork.Currency == CurrencyCodes.EUR) res = res + " (" + (1 / rate.Rate).ToString("F3") + ")";
        return res;
      }
    }

    /// <summary>
    /// 1. В базе транзакции хранятся неупорядоченные, поэтому при зачитывании в Rows берем их OrderBy
    /// 2. Во время работы формы я поддерживаю правильную сортированность Rows, вставляя новые строки в те места , где они и отображаются
    /// 3. При выходе из программы в БД записываются только изменения, значит новые транзакции добавятся в конец. См п.1
    /// </summary>
    public TransactionsViewModel()
    {
      ComboBoxesValues();
      Db.Transactions.Load();
      //      Rows = new ObservableCollection<Transaction>(Db.Transactions.Local.OrderBy(transaction => transaction.Timestamp));
      Rows = Db.Transactions.Local; // берем все-таки несортированную коллекцию
      SortedRows = CollectionViewSource.GetDefaultView(Rows);
      SortedRows.SortDescriptions.Add(new SortDescription("Timestamp", ListSortDirection.Ascending));
      SortedRows.MoveCurrentToLast();

      _transactionInWork = new Transaction();
    }

    private int ExtractPureOperationType(int param)
    {
      return param >= 900 ? param - 900 : param;
    }

    private bool HaveTheSameOperationType(Transaction a, Transaction b)
    { // и 1 и 901 это расход
      return (a.Operation == b.Operation || Math.Abs((int)a.Operation - (int)b.Operation) == 900);
    }

    /// <summary>
    /// Когда форма загружена, подписываемся на событие "Изменено свойство в инстансе TransactionInWork класса Transaction
    /// Такая возможность существует т.к. класс Transaction отнаследован от PropertyChangedBase
    /// Однако чтобы такое событие происходило, надо чтобы хотя бы одно свойство класса Transaction было нотифицирующим
    /// </summary>
    /// <param name="view"></param>
    protected override void OnViewLoaded(object view)
    {
      DisplayName = "Daily transactions";
      SelectedTransaction = (Transaction)SortedRows.CurrentItem;

      TransactionInWork.PropertyChanged += TransactionInWorkPropertyChanged;
      CanSaveTransactionChanges = false;
      CanCancelTransactionChanges = false;
    }

    /// <summary>
    /// Какое именно свойство в инстансе TransactionInWork класса Transaction можно узнать из  e.PropertyName ,
    /// но в данном случае нас это не интересует.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void TransactionInWorkPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      IsTransactionInWorkChanged = true;
      if (e.PropertyName == "Amount") { NotifyOfPropertyChange(() => AmountInUsd); }
    }

    #region  // кнопки операций над транзакциями

    public bool CanSaveTransactionChanges { get; set; }
    public bool CanCancelTransactionChanges { get; set; }

    public void SaveTransactionChanges()
    {
      var isTimestampChanged = SelectedTransaction.Timestamp.Date != TransactionInWork.Timestamp.Date;
      if (TransactionInWork.Amount2 == 0) TransactionInWork.Currency2 = null;
      SelectedTransaction.CloneFrom(TransactionInWork);
      if (isTimestampChanged)
      {
        MoveTransactionToRightPlace();
      }
      IsTransactionInWorkChanged = false;
      _isInAddTransactionMode = false;
    }

    public void CancelTransactionChanges()
    {
      if (_isInAddTransactionMode)
      {
        _isInAddTransactionMode = false;
        DeleteTransaction();
      }

      if (TransactionInWork.Operation != SelectedTransaction.Operation)
      {
        if ((int)SelectedTransaction.Operation >= 900)
          SelectedTransaction.Operation = (OperationType)((int)SelectedTransaction.Operation - 900);
        SelectedTabIndex = (int)SelectedTransaction.Operation;
      }
      TransactionInWork.CloneFrom(SelectedTransaction);
      IsTransactionInWorkChanged = false;
    }

    /// <summary>
    /// добавление новой транзакции в позицию за SelectedTransaction
    /// </summary>
    public void AddTransactionAfterSelected()
    {
      if (CanSaveTransactionChanges) SaveTransactionChanges();
      _isInAddTransactionMode = true;

      TransactionInWork = SelectedTransaction.Preform("SameDate");  // новая запись получит то же время что и выделенная + 1 минута

      SelectedTransaction = new Transaction();
      SelectedTransaction.Timestamp = TransactionInWork.Timestamp;
      SelectedTransaction.Operation = (OperationType)(900 + (int)SelectedTransaction.Operation);

      Rows.Add(SelectedTransaction);
      MoveDownTransactionsFromCurrentUntilHole();

      IsTransactionInWorkChanged = true;
    }

    private void MoveDownTransactionsFromCurrentUntilHole()
    {
      SortedRows.MoveCurrentTo(SelectedTransaction);
      var previous = (Transaction)SortedRows.CurrentItem; // только что вставленная заготовка

      if (!SortedRows.MoveCurrentToNext()) return; // шагнули за конец списка
      var current = (Transaction)SortedRows.CurrentItem;

      while (current.Timestamp.Date == previous.Timestamp.Date)
      {
        current.Timestamp = previous.Timestamp.AddMinutes(1);
        previous = current;
        if (!SortedRows.MoveCurrentToNext()) return; // шагнули за конец списка
        current = (Transaction)SortedRows.CurrentItem;
      }
    }

    /// <summary>
    /// добавление новой транзакции в конец списка, с датой следующей за последней
    /// </summary>
    public void AddNewDayTransaction()
    {
      if (CanSaveTransactionChanges) SaveTransactionChanges();
      _isInAddTransactionMode = true;

      SortedRows = CollectionViewSource.GetDefaultView(Rows);
      SortedRows.MoveCurrentToLast();
      TransactionInWork = ((Transaction)SortedRows.CurrentItem).Preform("NextDate");

      SelectedTransaction = new Transaction();
      SelectedTransaction.Timestamp = TransactionInWork.Timestamp;
      SelectedTransaction.Operation = (OperationType)(900 + (int)SelectedTransaction.Operation);

      Rows.Add(SelectedTransaction);

      IsTransactionInWorkChanged = true;
    }

    public void DeleteTransaction()
    {
      var transactionForRemoval = SelectedTransaction;

      SortedRows.MoveCurrentTo(SelectedTransaction);
      if (!SortedRows.MoveCurrentToNext())
      {
        SortedRows.MoveCurrentToPrevious();
        SortedRows.MoveCurrentToPrevious();
      }
      SelectedTransaction = (Transaction) SortedRows.CurrentItem;
      
      Rows.Remove(transactionForRemoval);
//      TransactionInWork.CloneFrom(SelectedTransaction);
      IsTransactionInWorkChanged = false;
    }

    #endregion // кнопки операций над транзакциями


    public void DecreaseTimestamp()
    {
      TransactionInWork.Timestamp = TransactionInWork.Timestamp.AddDays(-1);
    }

    public void IncreaseTimestamp()
    {
      TransactionInWork.Timestamp = TransactionInWork.Timestamp.AddDays(1);
    }

    public void MoveTransactionToRightPlace()
    {

    }

    public void MoveTransactionUp()
    {
      if (CanSaveTransactionChanges) SaveTransactionChanges();

      SortedRows.MoveCurrentTo(SelectedTransaction);

      var activeTransaction = (Transaction)SortedRows.CurrentItem;
      if (!SortedRows.MoveCurrentToPrevious()) return;                        // previous
      var nearbyTransaction = (Transaction)SortedRows.CurrentItem;

      activeTransaction.Timestamp = nearbyTransaction.Timestamp;
      nearbyTransaction.Timestamp = nearbyTransaction.Timestamp.AddMinutes(1);

      SortedRows.Refresh();
    }

    public void MoveTransactionDown()
    {
      if (CanSaveTransactionChanges) SaveTransactionChanges();

      SortedRows.MoveCurrentTo(SelectedTransaction);

      if (!SortedRows.MoveCurrentToNext()) return;                           // next
      var nearbyTransaction = (Transaction)SortedRows.CurrentItem;

      if (SelectedTransaction.Timestamp.Date == nearbyTransaction.Timestamp.Date)
      {
        SelectedTransaction.Timestamp = nearbyTransaction.Timestamp;
        nearbyTransaction.Timestamp = SelectedTransaction.Timestamp.AddMinutes(-1);
      }
      else
      {
        SelectedTransaction.Timestamp = nearbyTransaction.Timestamp.AddMinutes(1);
        MoveDownTransactionsFromCurrentUntilHole();
      }

      SortedRows.Refresh();
      SortedRows.MoveCurrentTo(SelectedTransaction);
    }

  }

}
