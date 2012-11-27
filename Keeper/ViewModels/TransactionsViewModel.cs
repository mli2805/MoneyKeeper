using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Windows.Controls;
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
    public List<Account> AccountsWhoGivesMeMoney { get; set; }
    public List<Account> AccountsWhoTakesMyMoney { get; set; }
    public List<Account> BankAccounts { get; set; }
    public List<Account> IncomeArticles { get; set; }
    public List<Account> ExpenseArticles { get; set; }

    public void ComboBoxesValues()
    {
      OperationTypes = Enum.GetValues(typeof(OperationType)).OfType<OperationType>().ToList();
      CurrencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();

      MyAccounts = (Db.Accounts.Local.Where(account => account.GetRootName() == "Мои")).ToList();
      AccountsWhoGivesMeMoney = (Db.Accounts.Local.Where(account => account.IsDescendantOf("ДеньгоДатели"))).ToList();
      AccountsWhoTakesMyMoney = (Db.Accounts.Local.Where(account => account.IsDescendantOf("ДеньгоПолучатели"))).ToList();
      BankAccounts = (Db.Accounts.Local.Where(account => account.IsDescendantOf("Банки"))).ToList();
      IncomeArticles = (Db.Accounts.Local.Where(account => account.GetRootName() == "Все доходы")).ToList();
      ExpenseArticles = (Db.Accounts.Local.Where(account => account.GetRootName() == "Все расходы")).ToList();
    }
    #endregion  

    public KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public ObservableCollection<Transaction> Rows { get; set; }

    private bool _isInTransactionSelectionProcess;
    private Transaction _selectedTransactionBeforeEditing;

    private int _selectedTabIndex;
    public int SelectedTabIndex
    {
      get { return _selectedTabIndex; }
      set
      {
        if (value == _selectedTabIndex) return;
        _selectedTabIndex = value;
        NotifyOfPropertyChange(() => SelectedTabIndex);
        _selectedTransaction.Operation = (OperationType) _selectedTabIndex;
        if (!_isInTransactionSelectionProcess)
        {
          _selectedTransaction.Debet = null;
          _selectedTransaction.Credit = null;
          _selectedTransaction.Article = null;
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
        NotifyOfPropertyChange(() => SelectedTransaction);
        NotifyOfPropertyChange(() => AmountInUsd);
        _isInTransactionSelectionProcess = true;
        SelectedTabIndex = (int) _selectedTransaction.Operation;
        _isInTransactionSelectionProcess = false;
        if (_selectedTransaction != null) _selectedTransactionBeforeEditing.SuckOut(_selectedTransaction);
      }
    }

    public string AmountInUsd
    {
      get
      {
        if (SelectedTransaction.Currency == CurrencyCodes.USD) return "";
        var rate = new CurrencyRate();
        
        rate =
          Db.CurrencyRates.Local.FirstOrDefault(
            currencyRate => ((currencyRate.BankDay == SelectedTransaction.Timestamp) && (currencyRate.Currency == SelectedTransaction.Currency))); 
        
        if (rate == null) return "отсутствует курс на эту дату";
        var res = (SelectedTransaction.Amount / (decimal)rate.Rate).ToString("F2") +"$ по курсу "+rate.Rate;
        if (SelectedTransaction.Currency == CurrencyCodes.EUR) res = res + " (" + (1/rate.Rate).ToString("F3")+ ")";
        return res;
      }
    }

    public TransactionsViewModel()
    {
      Db.Transactions.Load();
      Rows = Db.Transactions.Local;
      _selectedTransactionBeforeEditing = new Transaction();
      SelectedTransaction = Rows.Last();
      ComboBoxesValues();
    }

    protected override void OnViewLoaded(object view)
    {
      SelectedTransaction.PropertyChanged += SelectedTransactionPropertyChanged;
      CanCancelTransactionChanges = false;
    }

    void SelectedTransactionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      CanCancelTransactionChanges = true;
    }

    /// <summary>
    /// добавление новой транзакции в позицию за SelectedTransaction
    /// </summary>
    public void AddOnceMoreTransaction()
    {
      var selectedIndex = Rows.IndexOf(SelectedTransaction)+1;
      var preformTransaction = SelectedTransaction.Preform();
      SelectedTransaction = preformTransaction;
      Rows.Insert(selectedIndex,SelectedTransaction);
    }

    public void SaveTransaction()
    {
      _selectedTransactionBeforeEditing.SuckOut(SelectedTransaction);
      CanCancelTransactionChanges = false;
    }

    public void DeleteTransaction()
    {
      var transactionForRemoving = SelectedTransaction;
      var selectedIndex = Rows.IndexOf(SelectedTransaction);
      if (Rows.Count == selectedIndex+1) SelectedTransaction = Rows.ElementAt(--selectedIndex);
      else SelectedTransaction = Rows.ElementAt(++selectedIndex);
      Rows.Remove(transactionForRemoving);
    }

    private bool _canCancelTransactionChanges;
    public bool CanCancelTransactionChanges
    {
      get { return _canCancelTransactionChanges; }
      set
      {
        if (value.Equals(_canCancelTransactionChanges)) return;
        _canCancelTransactionChanges = value;
        NotifyOfPropertyChange(() => CanCancelTransactionChanges);
      }
    }

    public void CancelTransactionChanges()
    {
      SelectedTabIndex = (int)_selectedTransactionBeforeEditing.Operation;
      SelectedTransaction.SuckOut(_selectedTransactionBeforeEditing);
      CanCancelTransactionChanges = false;
    }

    public void DecreaseTimestamp()
    {
      SelectedTransaction.Timestamp = SelectedTransaction.Timestamp.AddDays(-1);
    }

    public void IncreaseTimestamp()
    {
      SelectedTransaction.Timestamp = SelectedTransaction.Timestamp.AddDays(1);
    }

  }


}
