using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Composition;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils.Accounts;
using Keeper.Utils.Balances;
using Keeper.Utils.CommonKeeper;
using Keeper.Utils.Rates;
using Keeper.ViewModels.TransactionViewFilters;

namespace Keeper.ViewModels
{
  [Export]
  [Shared] // для того чтобы в классе Transaction можно было обратиться к здешнему свойству SelectedTransaction
  public class TransactionViewModel : Screen
  {
    private readonly KeeperDb _db;

    private readonly RateExtractor _rateExtractor;
    private readonly BalanceCalculator _balanceCalculator;
    private readonly BalancesForTransactionsCalculator _balancesForTransactionsCalculator;
    private readonly AccountTreeStraightener _accountTreeStraightener;
    private readonly AssociationFinder _associationFinder;

    public static IWindowManager WindowManager { get { return IoC.Get<IWindowManager>(); } }
    public ObservableCollection<Transaction> Rows { get; set; }
    public bool IsCollectionChanged { get; set; }
    public ICollectionView SortedRows { get; set; }

    #region  фильтрация и переход к дате
    public List<AccountFilter> DebetFilterList { get; set; }
    public List<AccountFilter> CreditFilterList { get; set; }
    public List<AccountFilter> ArticleFilterList { get; set; }

    private OperationTypesFilter _selectedOperationTypeFilter;
    private AccountFilter _selectedDebetFilter;
    private AccountFilter _selectedCreditFilter;
    private AccountFilter _selectedArticleFilter;
    private string _commentFilter;
    private DateTime _dateToGo;

    private void InitializeFiltersLists()
    {
      DebetFilterList = new List<AccountFilter>();

      // <no filter>
      var filter = new AccountFilter();
      DebetFilterList.Add(filter);

      var debetAccounts = (_accountTreeStraightener.Flatten(_db.Accounts).Where(account =>
              (account.Is("Мои") || account.Is("Внешние")) && account.Children.Count == 0)).ToList();
      foreach (var account in debetAccounts)
      {
        filter = new AccountFilter(account);
        DebetFilterList.Add(filter);
      }

      CreditFilterList = DebetFilterList;

      ArticleFilterList = new List<AccountFilter>();
      // <no filter>
      filter = new AccountFilter();
      ArticleFilterList.Add(filter);

      var articleAccounts = (_accountTreeStraightener.Flatten(_db.Accounts).Where(account =>
              (account.Is("Все доходы") || account.Is("Все расходы")) && account.Children.Count == 0)).ToList();
      foreach (var account in articleAccounts)
      {
        filter = new AccountFilter(account);
        ArticleFilterList.Add(filter);
      }
    }

    public void ClearAllFilters()
    {
      SelectedOperationTypeFilter = OperationTypesFilerListForCombo.FilterList.First(f => !f.IsOn);
      SelectedDebetFilter = DebetFilterList.First(f => !f.IsOn);
      SelectedCreditFilter = CreditFilterList.First(f => !f.IsOn);
      SelectedArticleFilter = ArticleFilterList.First(f => !f.IsOn);
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
        SelectedTransaction = (from transaction in _db.Transactions
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

    #region Списки для комбобоксов
    private List<Account> _myAccounts;
    private List<CurrencyCodes> _currencyList;
    private List<Account> _myAccountsForShopping;
    private List<Account> _accountsWhoTakesMyMoney;
    private List<Account> _accountsWhoGivesMeMoney;
    private List<Account> _incomeArticles;
    private List<Account> _expenseArticles;
    private List<Account> _bankAccounts;

    public List<CurrencyCodes> CurrencyList
    {
      get { return _currencyList; }
      private set
      {
        if (Equals(value, _currencyList)) return;
        _currencyList = value;
        NotifyOfPropertyChange(() => CurrencyList);
      }
    }

    public List<Account> MyAccounts
    {
      get { return _myAccounts; }
      set
      {
        if (Equals(value, _myAccounts)) return;
        _myAccounts = value;
        NotifyOfPropertyChange(() => MyAccounts);
      }
    }

    public List<Account> MyAccountsForShopping
    {
      get { return _myAccountsForShopping; }
      set
      {
        if (Equals(value, _myAccountsForShopping)) return;
        _myAccountsForShopping = value;
        NotifyOfPropertyChange(() => MyAccountsForShopping);
      }
    }

    public List<Account> AccountsWhoTakesMyMoney
    {
      get { return _accountsWhoTakesMyMoney; }
      set
      {
        if (Equals(value, _accountsWhoTakesMyMoney)) return;
        _accountsWhoTakesMyMoney = value;
        NotifyOfPropertyChange(() => AccountsWhoTakesMyMoney);
      }
    }

    public List<Account> AccountsWhoGivesMeMoney
    {
      get { return _accountsWhoGivesMeMoney; }
      set
      {
        if (Equals(value, _accountsWhoGivesMeMoney)) return;
        _accountsWhoGivesMeMoney = value;
        NotifyOfPropertyChange(() => AccountsWhoGivesMeMoney);
      }
    }

    public List<Account> IncomeArticles
    {
      get { return _incomeArticles; }
      set
      {
        if (Equals(value, _incomeArticles)) return;
        _incomeArticles = value;
        NotifyOfPropertyChange(() => IncomeArticles);
      }
    }

    public List<Account> ExpenseArticles
    {
      get { return _expenseArticles; }
      set
      {
        if (Equals(value, _expenseArticles)) return;
        _expenseArticles = value;
        NotifyOfPropertyChange(() => ExpenseArticles);
      }
    }

    public List<Account> BankAccounts
    {
      get { return _bankAccounts; }
      set
      {
        if (Equals(value, _bankAccounts)) return;
        _bankAccounts = value;
        NotifyOfPropertyChange(() => BankAccounts);
      }
    }

    private void InitializeListsForCombobox()
    {
      CurrencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
      MyAccounts = (_accountTreeStraightener.Flatten(_db.Accounts).
        Where(account => account.Is("Мои") && account.Children.Count == 0 
                 || account.Name == "Для ввода стартовых остатков")).ToList();
      MyAccountsForShopping =
       (_accountTreeStraightener.Flatten(_db.Accounts).
          Where(account => account.Is("Мои") && account.Children.Count == 0 && !account.Is("Депозиты"))).ToList();
      BankAccounts = _accountTreeStraightener.Flatten(_db.Accounts).Where(a => a.Is("Банки") && a.Children.Count == 0).ToList();
      AccountsWhoTakesMyMoney = (_accountTreeStraightener.Flatten(_db.Accounts).
          Where(account => account.Is("ДеньгоПолучатели") && account.Children.Count == 0)).ToList();
      AccountsWhoGivesMeMoney = (_accountTreeStraightener.Flatten(_db.Accounts).
          Where(account => (account.Is("ДеньгоДатели") || account.Is("Банки")) && account.Children.Count == 0)).ToList();
      IncomeArticles =  (_accountTreeStraightener.Flatten(_db.Accounts).
          Where(account => account.Is("Все доходы") && account.Children.Count == 0)).ToList();
      ExpenseArticles = (_accountTreeStraightener.Flatten(_db.Accounts).
          Where(account => account.Is("Все расходы") && account.Children.Count == 0)).ToList();
    }

    #endregion

    #region группа свойств для биндинга селектов и др.

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
        AccommodateAccountsWithOperationType((OperationType)SelectedTabIndex);
      }
    }

    private void AccommodateAccountsWithOperationType(OperationType newOperationType)
    {
      TransactionInWork.Operation = newOperationType;
      switch (TransactionInWork.Operation)
      {
        case OperationType.Доход:
          if (!AccountsWhoGivesMeMoney.Contains(TransactionInWork.Debet))
          {
            TransactionInWork.Debet = AccountsWhoGivesMeMoney.First();
            TransactionInWork.Article = _associationFinder.GetAssociation(TransactionInWork.Debet);
          }
          if (!MyAccounts.Contains(TransactionInWork.Credit))
            TransactionInWork.Credit = MyAccounts.First();
          break;
        case OperationType.Расход:
          if (!AccountsWhoTakesMyMoney.Contains(TransactionInWork.Credit))
          {
            TransactionInWork.Credit = AccountsWhoTakesMyMoney.First();
            TransactionInWork.Article = _associationFinder.GetAssociation(TransactionInWork.Credit);
          }
          if (!MyAccountsForShopping.Contains(TransactionInWork.Debet))
            TransactionInWork.Debet = MyAccountsForShopping.First();
          break;
        case OperationType.Перенос:
          if (!MyAccounts.Contains(TransactionInWork.Debet))
            TransactionInWork.Debet = MyAccounts.First();
          if (!MyAccounts.Contains(TransactionInWork.Credit))
            TransactionInWork.Credit = MyAccounts.First();
          break;
        case OperationType.Обмен:
          if (!MyAccounts.Contains(TransactionInWork.Debet))
            TransactionInWork.Debet = MyAccounts.First();
          if (!BankAccounts.Contains(TransactionInWork.Credit))
            TransactionInWork.Credit = BankAccounts.First();
          break;
      }
    }

    public Transaction SelectedTransaction
    {
      set
      {
        if (_selectedTransaction != null) _selectedTransaction.SetIsSelectedWithoutNotification(false);

        _selectedTransaction = value ?? Rows.Last();
        _selectedTransaction.IsSelected = true;

        _isInTransactionSelectionProcess = true;
        TransactionInWork.CloneFrom(_selectedTransaction);
        _isInTransactionSelectionProcess = false;
        SelectedTabIndex = (int)_transactionInWork.Operation;
        NotifyOfPropertyChange(() => AmountInUsd);
        NotifyOfPropertyChange(() => DebetAccountBalance);
        NotifyOfPropertyChange(() => DebetAccountBalanceSecondCurrency);
        NotifyOfPropertyChange(() => CreditAccountBalance);
        NotifyOfPropertyChange(() => ExchangeRate);
        DayResults = _balancesForTransactionsCalculator.CalculateDayResults(SelectedTransaction.Timestamp);
        EndDayBalances = _balancesForTransactionsCalculator.EndDayBalances(SelectedTransaction.Timestamp);
      }
      get { return _selectedTransaction; }
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

    #region свойства Can для нескольких кнопок
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

    private bool _canFillInReceipt;
    public bool CanFillInReceipt
    {
      get { return _canFillInReceipt; }
      set
      {
        if (value.Equals(_canFillInReceipt)) return;
        _canFillInReceipt = value;
        NotifyOfPropertyChange(() => CanFillInReceipt);
      }
    }

    #endregion

    #region свойства для показа сумм остатков на счетах и перевода операции в доллары
    public string AmountInUsd
    {
      get
      {
        // одинарные операции не долларах
        if (TransactionInWork.Currency == CurrencyCodes.USD && SelectedTabIndex != 3) return "";
        const string res0 = "                                                                                ";

        var res1 = _rateExtractor.GetUsdEquivalentString(TransactionInWork.Amount, TransactionInWork.Currency, TransactionInWork.Timestamp);
        // одинарные операции не в остальных валютах
        if (SelectedTabIndex != 3) return res0 + res1;

        if (TransactionInWork.Currency2 == null) TransactionInWork.Currency2 = CurrencyCodes.BYR;
        var res2 = _rateExtractor.GetUsdEquivalent(TransactionInWork.Amount2, (CurrencyCodes)TransactionInWork.Currency2, TransactionInWork.Timestamp);
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
        if (TransactionInWork.Debet == null || !TransactionInWork.Debet.Is("Мои")) return "";

        var period = new Period(new DateTime(0), TransactionInWork.Timestamp.AddSeconds(-1));
        var balanceBefore = _balanceCalculator.GetBalanceInCurrency(TransactionInWork.Debet, period, TransactionInWork.Currency);

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
          _balanceCalculator.GetBalanceInCurrency(TransactionInWork.Debet, period, (CurrencyCodes)TransactionInWork.Currency2);

        return String.Format("{0:#,0} {2} -> {1:#,0} {2}",
             balanceBefore, balanceBefore + TransactionInWork.Amount2, TransactionInWork.Currency2.ToString().ToLower());
      }
    }

    public string CreditAccountBalance
    {
      get
      {
        if (TransactionInWork.Credit == null || !TransactionInWork.Credit.Is("Мои")) return "";

        var period = new Period(new DateTime(0), TransactionInWork.Timestamp.AddSeconds(-1));
        var balanceBefore = _balanceCalculator.GetBalanceInCurrency(TransactionInWork.Credit, period, TransactionInWork.Currency);

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

    [ImportingConstructor]
    public TransactionViewModel(KeeperDb db, RateExtractor rateExtractor, BalanceCalculator balanceCalculator,
      BalancesForTransactionsCalculator balancesForTransactionsCalculator, AccountTreeStraightener accountTreeStraightener)
    {
      _db = db;

      _rateExtractor = rateExtractor;
      _balanceCalculator = balanceCalculator;
      _balancesForTransactionsCalculator = balancesForTransactionsCalculator;
      _accountTreeStraightener = accountTreeStraightener;
      _associationFinder = new AssociationFinder(_db);
      InitializeListsForCombobox();
      TransactionInWork = new Transaction();
      Rows = _db.Transactions;
      Rows.CollectionChanged += RowsCollectionChanged;
      IsCollectionChanged = false;
      InitializeFiltersLists();
      InitializeSelectedTransactionIndex();
    }

    void RowsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      IsCollectionChanged = true;
    }

    private void InitializeSelectedTransactionIndex()
    {
      foreach (var transaction in _db.Transactions)
      {
        if (transaction.IsSelected)
        {
          SelectedTransactionIndex = Rows.IndexOf(transaction);
          return;
        }
      }
      SelectedTransactionIndex = Rows.Count - 1;
      Rows.Last().IsSelected = true;
    }

    /// <summary>
    /// Когда форма загружена, подписываемся на событие "Изменено свойство в инстансе TransactionInWork класса Transaction
    /// Такая возможность существует т.к. класс Transaction отнаследован от PropertyChangedBase
    /// Однако чтобы такое событие происходило, надо чтобы хотя бы одно свойство класса Transaction было нотифицирующим
    /// </summary>
    /// <param name="view"></param>
    protected override void OnViewLoaded(object view)
    {
      InitializeListsForCombobox();
      DisplayName = "Ежедневные операции";

      SortedRows = CollectionViewSource.GetDefaultView(Rows);
      SortedRows.SortDescriptions.Add(new SortDescription("Timestamp", ListSortDirection.Ascending));
      ClearAllFilters();
      SortedRows.Filter += OnFilter;

      TransactionInWork.PropertyChanged += TransactionInWorkPropertyChanged;
      IsTransactionInWorkChanged = false;
      CanSaveTransactionChanges = false;
      CanCancelTransactionChanges = false;
      CanFillInReceipt = false;
      IsInAddTransactionMode = false;
    }

    void TransactionInWorkPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (_isInTransactionSelectionProcess) return;

      IsTransactionInWorkChanged = true;

      if (e.PropertyName == "Comment") return;
      if (e.PropertyName == "Debet" && TransactionInWork.Operation == OperationType.Доход && IsInAddTransactionMode)
        TransactionInWork.Article = _associationFinder.GetAssociation(TransactionInWork.Debet);
      if (e.PropertyName == "Credit" && TransactionInWork.Operation == OperationType.Расход && IsInAddTransactionMode)
        TransactionInWork.Article = _associationFinder.GetAssociation(TransactionInWork.Credit);

      if (e.PropertyName == "Amount" || e.PropertyName == "Currency" ||
          e.PropertyName == "Amount2" || e.PropertyName == "Currency2")
      {
        NotifyOfPropertyChange(() => AmountInUsd);
        DayResults = _balancesForTransactionsCalculator.CalculateDayResults(SelectedTransaction.Timestamp);
      }

      NotifyOfPropertyChange(() => DebetAccountBalance);
      NotifyOfPropertyChange(() => DebetAccountBalanceSecondCurrency);
      NotifyOfPropertyChange(() => CreditAccountBalance);
      NotifyOfPropertyChange(() => ExchangeRate);
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
        var transactionBefore = _db.Transactions.Where(
          transaction => transaction.Timestamp.Date == TransactionInWork.Timestamp.Date).OrderBy(
            transaction => transaction.Timestamp).LastOrDefault();

        SelectedTransaction.CloneFrom(TransactionInWork);
        SelectedTransaction.Timestamp = transactionBefore == null ? SelectedTransaction.Timestamp.AddHours(9) : transactionBefore.Timestamp.AddMinutes(1);
        SortedRows.Refresh();
      }
      else SelectedTransaction.CloneFrom(TransactionInWork);

      DayResults = _balancesForTransactionsCalculator.CalculateDayResults(SelectedTransaction.Timestamp);
      EndDayBalances = _balancesForTransactionsCalculator.EndDayBalances(SelectedTransaction.Timestamp);
      IsTransactionInWorkChanged = false;
      IsInAddTransactionMode = false;
      CanFillInReceipt = false;
      IsCollectionChanged = true;
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
      CanFillInReceipt = false;
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
      CanFillInReceipt = true;
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
      CanFillInReceipt = true;
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
      CanFillInReceipt = true;
    }

    public void DeleteTransaction()
    {
      var transactionForRemoval = SelectedTransaction;

      if (SelectedTransactionIndex == 0) SelectedTransactionIndex++; else SelectedTransactionIndex--;
      Rows.Remove(transactionForRemoval);

      DayResults = _balancesForTransactionsCalculator.CalculateDayResults(SelectedTransaction.Timestamp);
      EndDayBalances = _balancesForTransactionsCalculator.EndDayBalances(SelectedTransaction.Timestamp);
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
      var receiptViewModel = new ReceiptViewModel(TransactionInWork.Timestamp, TransactionInWork.Credit.Name,
                                     TransactionInWork.Currency, TransactionInWork.Amount, TransactionInWork.Article, ExpenseArticles);
      WindowManager.ShowDialog(receiptViewModel);
      if (receiptViewModel.Result) // добавить транзакции
      {
        foreach (var tuple in receiptViewModel.Expense)
        {
          TransactionInWork.Amount = tuple.Item1;
          TransactionInWork.Article = tuple.Item2;
          TransactionInWork.Comment = tuple.Item3;
          SaveTransactionChanges();
          AddTransactionAfterSelected();
        }
        CancelTransactionChanges();
      }
    }

  }
}

