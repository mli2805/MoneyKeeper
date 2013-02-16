
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils;

namespace Keeper.ViewModels
{
  [Export, PartCreationPolicy(CreationPolicy.Shared)] // для того чтобы в классе Transaction можно было обратиться к здешнему свойству SelectedTransaction
  public class TransactionViewModel : Screen
  {
    public static KeeperTxtDb Db { get { return IoC.Get<KeeperTxtDb>(); } }
    public static IWindowManager WindowManager { get { return IoC.Get<IWindowManager>(); } }
    public ObservableCollection<Transaction> Rows { get; set; }
    public ICollectionView SortedRows { get; set; }

    #region  // фильтрация и переход к дате
    private OperationTypesFilter _selectedOperationTypeFilter;
    private AccountFilter _selectedDebetFilter;
    private AccountFilter _selectedCreditFilter;
    private AccountFilter _selectedArticleFilter;
    private string _commentFilter;
    private DateTime _dateToGo;

    public void ClearAllFilters()
    {
      SelectedOperationTypeFilter = OperationTypesFilerListForCombo.FilterList.First(f => !f.IsOn);
      SelectedDebetFilter = FilterListsForComboboxes.DebetFilterList.First(f => !f.IsOn);
      SelectedCreditFilter = FilterListsForComboboxes.CreditFilterList.First(f => !f.IsOn);
      SelectedArticleFilter = FilterListsForComboboxes.ArticleFilterList.First(f => !f.IsOn);
      CommentFilter = "";
    }

    private bool OnFilter(object o)
    {
      var transaction = (Transaction)o;
      if (SelectedOperationTypeFilter.IsOn && transaction.Operation != SelectedOperationTypeFilter.Operation) return false;
      if (SelectedDebetFilter.IsOn && transaction.Debet != SelectedDebetFilter.WantedAccount) return false;
      if (SelectedCreditFilter.IsOn && transaction.Credit != SelectedCreditFilter.WantedAccount) return false;
      if (SelectedArticleFilter.IsOn && transaction.Article != SelectedArticleFilter.WantedAccount) return false;
      if (CommentFilter != "" && transaction.Comment.IndexOf(CommentFilter, StringComparison.Ordinal) == -1) return false;

      return true;
    }

    public DateTime DateToGo
    {
      get { return _dateToGo; }
      set
      {
        if (value.Equals(_dateToGo)) return;
        _dateToGo = value;
        NotifyOfPropertyChange(() => DateToGo);
        SelectedTransaction = (from transaction in Db.Transactions
                               where transaction.Timestamp > DateToGo
                               select transaction).FirstOrDefault() ?? Rows.Last();
      }
    }

    public OperationTypesFilter SelectedOperationTypeFilter
    {
      get { return _selectedOperationTypeFilter; }
      set
      {
        if (Equals(value, _selectedOperationTypeFilter)) return;
        _selectedOperationTypeFilter = value;
        NotifyOfPropertyChange(() => SelectedOperationTypeFilter);
        var view = CollectionViewSource.GetDefaultView(SortedRows);
        view.Refresh();
      }
    }

    public AccountFilter SelectedDebetFilter
    {
      get { return _selectedDebetFilter; }
      set
      {
        if (Equals(value, _selectedDebetFilter)) return;
        _selectedDebetFilter = value;
        NotifyOfPropertyChange(() => SelectedDebetFilter);
        var view = CollectionViewSource.GetDefaultView(SortedRows);
        view.Refresh();
      }
    }

    public AccountFilter SelectedCreditFilter
    {
      get { return _selectedCreditFilter; }
      set
      {
        if (Equals(value, _selectedCreditFilter)) return;
        _selectedCreditFilter = value;
        NotifyOfPropertyChange(() => SelectedCreditFilter);
        var view = CollectionViewSource.GetDefaultView(SortedRows);
        view.Refresh();
      }
    }

    public AccountFilter SelectedArticleFilter
    {
      get { return _selectedArticleFilter; }
      set
      {
        if (Equals(value, _selectedArticleFilter)) return;
        _selectedArticleFilter = value;
        NotifyOfPropertyChange(() => SelectedArticleFilter);
        var view = CollectionViewSource.GetDefaultView(SortedRows);
        view.Refresh();
      }
    }

    public string CommentFilter
    {
      get { return _commentFilter; }
      set
      {
        if (value == _commentFilter) return;
        _commentFilter = value;
        NotifyOfPropertyChange(() => CommentFilter);
        var view = CollectionViewSource.GetDefaultView(SortedRows);
        view.Refresh();
      }
    }
    #endregion

    #region // группа свойств для биндинга селектов и др.
    private bool _isInTransactionSelectionProcess;
    private int _selectedTabIndex;
    private Transaction _selectedTransaction;
    private int _selectedTransactionIndex;
    private bool _isTransactionInWorkChanged;
    private Transaction _transactionInWork;
    private bool _isInAddTransactionMode;

    public int SelectedTabIndex
    {
      get { return _selectedTabIndex; }
      set
      {
        if (value == _selectedTabIndex) return;
        _selectedTabIndex = value;
        NotifyOfPropertyChange(() => SelectedTabIndex);
        TransactionInWork.Operation = (OperationType)SelectedTabIndex;
      }
    }

    public Transaction SelectedTransaction
    {
      get { return _selectedTransaction; }
      set
      {
        if (value == null)
        {
          MessageBox.Show("SelectedTransaction is null!");
          return;
        }
        _selectedTransaction = value;
        _isInTransactionSelectionProcess = true;
        TransactionInWork.CloneFrom(_selectedTransaction);
        _isInTransactionSelectionProcess = false;
        SelectedTabIndex = (int)_transactionInWork.Operation;
        NotifyOfPropertyChange(() => AmountInUsd);
        NotifyOfPropertyChange(() => DebetAccountBalance);
        NotifyOfPropertyChange(() => DebetAccountBalanceSecondCurrency);
        NotifyOfPropertyChange(() => CreditAccountBalance);
        NotifyOfPropertyChange(() => ExchangeRate);
        DayResults = Balance.CalculateDayResults(SelectedTransaction.Timestamp);
        EndDayBalances = Balance.EndDayBalances(SelectedTransaction.Timestamp);
      }
    }

    public int SelectedTransactionIndex
    {
      get { return _selectedTransactionIndex; }
      set
      {
        if (value == _selectedTransactionIndex) return;
        _selectedTransactionIndex = value;
        NotifyOfPropertyChange(() => SelectedTransactionIndex);
      }
    }

    public bool IsTransactionInWorkChanged
    {
      get { return _isTransactionInWorkChanged; }
      set
      {
        if (value.Equals(_isTransactionInWorkChanged)) return;
        _isTransactionInWorkChanged = value;
        CanSaveTransactionChanges = value;
        CanCancelTransactionChanges = value;
        NotifyOfPropertyChange(() => CanSaveTransactionChanges);
        NotifyOfPropertyChange(() => CanCancelTransactionChanges);
      }
    }

    public Transaction TransactionInWork
    {
      get { return _transactionInWork; }
      set
      {
        if (Equals(value, _transactionInWork)) return;
        _transactionInWork = value;
        SelectedTabIndex = (int)_transactionInWork.Operation;
        NotifyOfPropertyChange(() => TransactionInWork);
      }
    }

    public bool IsInAddTransactionMode
    {
      get { return _isInAddTransactionMode; }
      set
      {
        _isInAddTransactionMode = value;
        if (!value) CanEditDate = true;
      }
    }
    #endregion

    #region // свойства Can для нескольких кнопок
    private bool _canEditDate;
    public bool CanSaveTransactionChanges { get; set; }
    public bool CanCancelTransactionChanges { get; set; }
    public bool CanEditDate
    {
      get { return _canEditDate; }
      set
      {
        if (value.Equals(_canEditDate)) return;
        _canEditDate = value;
        NotifyOfPropertyChange(() => CanEditDate);
      }
    }
    #endregion

    #region // свойства для показа сумм остатков на счетах и перевода операции в доллары
    public string AmountInUsd
    {
      get
      {
        // одинарные операции не долларах
        if (TransactionInWork.Currency == CurrencyCodes.USD && SelectedTabIndex != 3) return "";
        const string res0 = "                                                                                ";

        var res1 = Rate.GetUsdEquivalent(TransactionInWork.Amount, TransactionInWork.Currency, TransactionInWork.Timestamp);
        // одинарные операции не в остальных валютах
        if (SelectedTabIndex != 3) return res0 + res1;

        if (TransactionInWork.Currency2 == null) TransactionInWork.Currency2 = CurrencyCodes.BYR;
        var res2 = Rate.GetUsdEquivalent(TransactionInWork.Amount2, (CurrencyCodes)TransactionInWork.Currency2, TransactionInWork.Timestamp);
        // обменные операции: доллары на другую валюту
        if (SelectedTabIndex == 3 && TransactionInWork.Currency == CurrencyCodes.USD) return res0 + res2;
        // обменные операции: другая валюта на доллары
        if (SelectedTabIndex == 3 && TransactionInWork.Currency2 == CurrencyCodes.USD) return res1;
        // обменные операции: не доллары на не доллары
        return res1 + "                                 " + res2;
      }
    }

    public string DebetAccountBalance
    {
      get
      {
        if (TransactionInWork.Debet == null || !TransactionInWork.Debet.IsDescendantOf("Мои")) return "";

        var period = new Period(new DateTime(0), TransactionInWork.Timestamp.AddSeconds(-1));
        var balanceBefore = Balance.GetBalanceInCurrency(TransactionInWork.Debet, period, TransactionInWork.Currency);

        return String.Format("{0:#,0} {2} -> {1:#,0} {2}",
             balanceBefore, balanceBefore - TransactionInWork.Amount, TransactionInWork.Currency.ToString().ToLower());
      }
    }

    public string DebetAccountBalanceSecondCurrency
    {
      get
      {
        if (TransactionInWork.Debet == null || TransactionInWork.Operation != OperationType.Обмен
                                                           || TransactionInWork.Currency2 == null) return "";

        var period = new Period(new DateTime(0), TransactionInWork.Timestamp.AddSeconds(-1));
        var balanceBefore =
          Balance.GetBalanceInCurrency(TransactionInWork.Debet, period, (CurrencyCodes)TransactionInWork.Currency2);

        return String.Format("{0:#,0} {2} -> {1:#,0} {2}",
             balanceBefore, balanceBefore + TransactionInWork.Amount2, TransactionInWork.Currency2.ToString().ToLower());
      }
    }

    public string CreditAccountBalance
    {
      get
      {
        if (TransactionInWork.Credit == null || !TransactionInWork.Credit.IsDescendantOf("Мои")) return "";

        var period = new Period(new DateTime(0), TransactionInWork.Timestamp.AddSeconds(-1));
        var balanceBefore = Balance.GetBalanceInCurrency(TransactionInWork.Credit, period, TransactionInWork.Currency);

        return String.Format("{0:#,0} {2} -> {1:#,0} {2}",
             balanceBefore, balanceBefore + TransactionInWork.Amount, TransactionInWork.Currency.ToString().ToLower());
      }
    }

    public string ExchangeRate
    {
      get
      {
        if (TransactionInWork.Operation != OperationType.Обмен) return "";

        if (TransactionInWork.Currency == CurrencyCodes.BYR)
        {
          return TransactionInWork.Amount2 != 0 ? String.Format("обменный курс - {0:#,0}", TransactionInWork.Amount / TransactionInWork.Amount2) : "";
        }
        if (TransactionInWork.Currency2 == CurrencyCodes.BYR)
        {
          return TransactionInWork.Amount != 0 ? String.Format("обменный курс - {0:#,0}", TransactionInWork.Amount2 / TransactionInWork.Amount) : "";
        }
        if (TransactionInWork.Currency == CurrencyCodes.USD)
        {
          return TransactionInWork.Amount != 0 ? String.Format("обменный курс - {0:#,00}", TransactionInWork.Amount2 / TransactionInWork.Amount) : "";
        }
        if (TransactionInWork.Currency2 == CurrencyCodes.USD)
        {
          return TransactionInWork.Amount2 != 0 ? String.Format("обменный курс - {0:#,00}", TransactionInWork.Amount / TransactionInWork.Amount2) : "";
        }
        return "не понял!";
      }
    }

    private List<string> _dayResults;

    public List<string> DayResults
    {
      get { return _dayResults; }
      set
      {
        if (Equals(value, _dayResults)) return;
        _dayResults = value;
        NotifyOfPropertyChange(() => DayResults);
      }
    }

    private string _endDayBalances;
    public string EndDayBalances
    {
      get { return _endDayBalances; }
      set
      {
        if (value == _endDayBalances) return;
        _endDayBalances = value;
        NotifyOfPropertyChange(() => EndDayBalances);
      }
    }

    #endregion

    public TransactionViewModel()
    {
      TransactionInWork = new Transaction();
      Rows = Db.Transactions;
      SelectedTransactionIndex = Rows.Count - 1;
    }

    /// <summary>
    /// Когда форма загружена, подписываемся на событие "Изменено свойство в инстансе TransactionInWork класса Transaction
    /// Такая возможность существует т.к. класс Transaction отнаследован от PropertyChangedBase
    /// Однако чтобы такое событие происходило, надо чтобы хотя бы одно свойство класса Transaction было нотифицирующим
    /// </summary>
    /// <param name="view"></param>
    protected override void OnViewLoaded(object view)
    {
      DisplayName = "Ежедневные операции";

      SortedRows = CollectionViewSource.GetDefaultView(Rows);
      SortedRows.SortDescriptions.Add(new SortDescription("Timestamp", ListSortDirection.Ascending));
      ClearAllFilters();
      SortedRows.Filter += OnFilter;

      TransactionInWork.PropertyChanged += TransactionInWorkPropertyChanged;
      CanSaveTransactionChanges = false;
      CanCancelTransactionChanges = false;
      IsInAddTransactionMode = false;
    }

    /// <summary>
    /// Какое именно свойство в инстансе TransactionInWork класса Transaction можно узнать из  e.PropertyName ,
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void TransactionInWorkPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!_isInTransactionSelectionProcess)
      {
        IsTransactionInWorkChanged = true;

        if (e.PropertyName == "Comment") return;

        if (e.PropertyName == "Debet" && TransactionInWork.Operation == OperationType.Доход && IsInAddTransactionMode)
          TransactionInWork.Article = AssociatedArticles.GetAssociation(TransactionInWork.Debet);
        if (e.PropertyName == "Credit" && TransactionInWork.Operation == OperationType.Расход && IsInAddTransactionMode)
          TransactionInWork.Article = AssociatedArticles.GetAssociation(TransactionInWork.Credit);

        if (e.PropertyName == "Amount" || e.PropertyName == "Currency" ||
                e.PropertyName == "Amount2" || e.PropertyName == "Currency2")
        {
          NotifyOfPropertyChange(() => AmountInUsd);
          DayResults = Balance.CalculateDayResults(SelectedTransaction.Timestamp);
        }

        NotifyOfPropertyChange(() => DebetAccountBalance);
        NotifyOfPropertyChange(() => DebetAccountBalanceSecondCurrency);
        NotifyOfPropertyChange(() => CreditAccountBalance);
        NotifyOfPropertyChange(() => ExchangeRate);
      }
    }

    protected override void OnDeactivate(bool close)
    {
      if (IsInAddTransactionMode) DeleteTransaction();
      base.OnDeactivate(close);
    }

    public void MoveTransactionUp()
    {
      var currentTransaction = SelectedTransaction;
      SelectedTransactionIndex--;
      var nearbyTransaction = SelectedTransaction;

      currentTransaction.Timestamp = nearbyTransaction.Timestamp;
      nearbyTransaction.Timestamp = nearbyTransaction.Timestamp.AddMinutes(1);

      SortedRows.Refresh();
      SelectedTransactionIndex--;
    }

    public void MoveTransactionDown()
    {
      var currentTransaction = SelectedTransaction;
      SelectedTransactionIndex++;
      var nearbyTransaction = SelectedTransaction;

      currentTransaction.Timestamp = nearbyTransaction.Timestamp;
      nearbyTransaction.Timestamp = nearbyTransaction.Timestamp.AddMinutes(-1);

      SortedRows.Refresh();
      SelectedTransactionIndex++;
    }

    public void SaveTransactionChanges()
    {
      //     из-за смены типа операции во время ввода/редактирования в некоторых полях TransactionInWork могли остаться ненужные данные
      CleanUselessFieldsBeforeSave();

      var isDateChanged = SelectedTransaction.Timestamp != TransactionInWork.Timestamp;

      if (isDateChanged)
      {
        //            все же SQL запрос пока наглядней чем LINQ форма
        //      var transactionBefore = (from transaction in Db.Transactions
        //                             where transaction.Timestamp.Date == TransactionInWork.Timestamp.Date
        //                             orderby transaction.Timestamp
        //                             select transaction).LastOrDefault();

        var transactionBefore = Db.Transactions.Where(
          transaction => transaction.Timestamp.Date == TransactionInWork.Timestamp.Date).OrderBy(
            transaction => transaction.Timestamp).LastOrDefault();

        SelectedTransaction.CloneFrom(TransactionInWork);
        SelectedTransaction.Timestamp = transactionBefore == null ? SelectedTransaction.Timestamp.AddHours(9) : transactionBefore.Timestamp.AddMinutes(1);
        SortedRows.Refresh();
      }
      else SelectedTransaction.CloneFrom(TransactionInWork);

      DayResults = Balance.CalculateDayResults(SelectedTransaction.Timestamp);
      EndDayBalances = Balance.EndDayBalances(SelectedTransaction.Timestamp);
      IsTransactionInWorkChanged = false;
      IsInAddTransactionMode = false;
    }

    private void CleanUselessFieldsBeforeSave()
    {
      if (TransactionInWork.Operation != OperationType.Обмен)
      {
        TransactionInWork.Amount2 = 0;
        TransactionInWork.Currency2 = null;
      }

      if (TransactionInWork.Operation != OperationType.Доход && TransactionInWork.Operation != OperationType.Расход)
      {
        TransactionInWork.Article = null;
      }
    }

    public void EscapeButtonPressed()
    {
      CancelTransactionChanges();
    }

    public void CancelTransactionChanges()
    {
      if (IsInAddTransactionMode) DeleteTransaction();

      TransactionInWork.CloneFrom(SelectedTransaction);
      SelectedTabIndex = (int)TransactionInWork.Operation;
      IsTransactionInWorkChanged = false;
      IsInAddTransactionMode = false;
    }


    public void AddTransactionBeforeSelected()
    {
      if (CanSaveTransactionChanges) SaveTransactionChanges();
      IsInAddTransactionMode = true;
      CanEditDate = false;

      var newTransaction = SelectedTransaction.Preform("SameDate");
      IncreaseNextTransactionTime();

      Rows.Add(newTransaction);
      SelectedTransactionIndex--;
      IsTransactionInWorkChanged = true;
    }

    /// <summary>
    /// Функция, которая обязательно увеличит Timestamp нынешней выделенной транзакции
    /// а если надо, то и последующих 
    /// </summary>
    private void IncreaseNextTransactionTime()
    {
      var workTransaction = SelectedTransaction;
      SelectedTransactionIndex++;
      if (workTransaction.Timestamp.AddMinutes(1) == SelectedTransaction.Timestamp) IncreaseNextTransactionTime();
      SelectedTransactionIndex--;
      SelectedTransaction.Timestamp = SelectedTransaction.Timestamp.AddMinutes(1);
    }

    public void AddTransactionAfterSelected()
    {
      if (CanSaveTransactionChanges) SaveTransactionChanges();
      IsInAddTransactionMode = true;
      CanEditDate = false;

      var newTransaction = SelectedTransaction.Preform("SameDatePlusMinite");

      if (SelectedTransactionIndex != Rows.Count - 1)
      {
        SelectedTransactionIndex++;
        if (newTransaction.Timestamp == SelectedTransaction.Timestamp)
          IncreaseNextTransactionTime();
        SelectedTransactionIndex--;
      }

      Rows.Add(newTransaction);
      SelectedTransactionIndex++;
      IsTransactionInWorkChanged = true;
    }

    public void AddNewDayTransaction()
    {
      if (CanSaveTransactionChanges) SaveTransactionChanges();
      IsInAddTransactionMode = true;

      SelectedTransactionIndex = Rows.Count - 1;
      var newTransaction = SelectedTransaction.Preform("NextDate");

      Rows.Add(newTransaction);
      SelectedTransactionIndex++;
      IsTransactionInWorkChanged = true;
    }

    public void DeleteTransaction()
    {
      var transactionForRemoval = SelectedTransaction;

      if (SelectedTransactionIndex != Rows.Count - 1) SelectedTransactionIndex++; else SelectedTransactionIndex--;
      Rows.Remove(transactionForRemoval);

      DayResults = Balance.CalculateDayResults(SelectedTransaction.Timestamp);
      EndDayBalances = Balance.EndDayBalances(SelectedTransaction.Timestamp);
      IsInAddTransactionMode = false;
    }

    public void IncreaseTimestamp()
    {
      TransactionInWork.Timestamp = TransactionInWork.Timestamp.AddDays(1);
    }

    public void DecreaseTimestamp()
    {
      TransactionInWork.Timestamp = TransactionInWork.Timestamp.AddDays(-1);
    }

    public void FillInReceipt()
    {
      var receiptViewModel = new ReceiptViewModel();
      WindowManager.ShowDialog(receiptViewModel);
      if (receiptViewModel.Result)
      {
        // добавить транзакции
      }
    }
  }
}

