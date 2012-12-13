﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Caliburn.Micro;
using Keeper.DomainModel;
using System.Data.Entity;

namespace Keeper.ViewModels
{
  [Export(typeof(IShell))]
  [Export(typeof(TransactionViewModel)), PartCreationPolicy(CreationPolicy.Shared)]
  public class TransactionViewModel : Screen, IShell
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

    private bool _isInTransactionSelectionProcess;
    private Transaction _selectedTransaction;
    private int _selectedTransactionIndex;
    private Transaction _transactionInWork;
    private int _selectedTabIndex;
    private bool _isTransactionInWorkChanged;
    private bool _isInAddTransactionMode;

    public KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public ObservableCollection<Transaction> Rows { get; set; }
    public ICollectionView SortedRows { get; set; }
    public Transaction SelectedTransaction
    {
      get { return _selectedTransaction; }
      set
      {
        _selectedTransaction = value;
        _isInTransactionSelectionProcess = true;
        TransactionInWork.CloneFrom(_selectedTransaction);
        _isInTransactionSelectionProcess = false;
        SelectedTabIndex = (int)_transactionInWork.Operation;
        NotifyOfPropertyChange(() => AmountInUsd);
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

    public int SelectedTabIndex
    {
      get { return _selectedTabIndex; }
      set
      {
        if (value == _selectedTabIndex) return;
        _selectedTabIndex = value;
        NotifyOfPropertyChange(() => SelectedTabIndex);
        TransactionInWork.Operation = (OperationType) SelectedTabIndex;
      }
    }

    public bool CanSaveTransactionChanges { get; set; }
    public bool CanCancelTransactionChanges { get; set; }

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

    public TransactionViewModel()
    {
      ComboBoxesValues();
      TransactionInWork = new Transaction();
      Db.Transactions.Load();
      Rows = Db.Transactions.Local;
      SelectedTransactionIndex = Rows.Count - 1;

      SortedRows = CollectionViewSource.GetDefaultView(Rows);
      SortedRows.SortDescriptions.Add(new SortDescription("Timestamp", ListSortDirection.Ascending));
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
      if (!_isInTransactionSelectionProcess)
      {
        IsTransactionInWorkChanged = true;
        if (e.PropertyName == "Amount" || e.PropertyName == "Currency")
        {
          NotifyOfPropertyChange(() => AmountInUsd);
        }
      }
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
      SelectedTransaction.CloneFrom(TransactionInWork);
      IsTransactionInWorkChanged = false;
    }

    public void CancelTransactionChanges()
    {
      if (_isInAddTransactionMode) DeleteTransaction();

      TransactionInWork.CloneFrom(SelectedTransaction);
      SelectedTabIndex = (int)TransactionInWork.Operation;
      IsTransactionInWorkChanged = false;
    }

 
    public void AddTransactionBeforeSelected()
    {
      if (CanSaveTransactionChanges) SaveTransactionChanges();
      _isInAddTransactionMode = true;

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
      _isInAddTransactionMode = true;

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
      _isInAddTransactionMode = true;

      SelectedTransactionIndex = Rows.Count - 1;
      var newTransaction = SelectedTransaction.Preform("NextDate");

      Rows.Add(newTransaction);
      SelectedTransactionIndex++;
      IsTransactionInWorkChanged = true;
    }
    
    public void DeleteTransaction()
    {
      var transactionForRemoval = SelectedTransaction;

      if (SelectedTransactionIndex != Rows.Count-1) SelectedTransactionIndex++; else SelectedTransactionIndex--;
      Rows.Remove(transactionForRemoval);

      _isInAddTransactionMode = false;

    }

  }
}
