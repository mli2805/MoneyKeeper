﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Keeper.DbInputOutput;
using Keeper.DomainModel;
using Keeper.Properties;
using Keeper.Utils;
using Keeper.Utils.Diagram;
using Brushes = System.Windows.Media.Brushes;


namespace Keeper.ViewModels
{
  [Export(typeof(IShell))] // это для загрузчика, который ищет главное окно проги
  [Export(typeof(ShellViewModel)), PartCreationPolicy(CreationPolicy.Shared)] // это для класса Account чтобы засунуть в свойство SelectedAccount 
  public class ShellViewModel : Screen, IShell, IPartImportsSatisfiedNotification
  {
    [Import]
    public IWindowManager WindowManager { get; set; }

    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }
    private AccountTreesFunctions _accountTreesFunctions;
    private BalancesForShellCalculator _balanceCalculator;
    private DiagramDataCtors _diagramDataCtor;
    private DiagramDataExtractor _diagramDataCalculator;

    #region // поля/свойства в классе Модели к которым биндятся визуальные элементы из Вью

    // чисто по приколу, label на вьюхе, которая по ходу программы может меняться - поэтому свойство с нотификацией
    private string _message;
    private string _statusBarItem0;
    private Account _selectedAccount;
    private int _openedAccountPage;
    private string _accountBalanceInUsd;
    private Visibility _isDeposit;
    private bool _isDbLoadingSuccessed;

    private DateTime _balanceDate;
    private Period _paymentsPeriod;
    private Visibility _balanceDateSelectControl;
    private Visibility _paymentsPeriodSelectControl;

    public string Message
    {
      get { return _message; }
      set
      {
        if (value == _message) return;
        _message = value;
        NotifyOfPropertyChange(() => Message);
      }
    }

    public string StatusBarItem0
    {
      get { return _statusBarItem0; }
      set
      {
        if (value.Equals(_statusBarItem0)) return;
        _statusBarItem0 = value;
        NotifyOfPropertyChange(() => StatusBarItem0);
      }
    }

    public string AccountBalanceInUsd
    {
      get { return _accountBalanceInUsd; }
      set
      {
        if (value == _accountBalanceInUsd) return;
        _accountBalanceInUsd = value;
        NotifyOfPropertyChange(() => AccountBalanceInUsd);
      }
    }

    // во ViewModel создается public property к которому будет биндиться компонент из View
    // далее содержимое этого свойства изменяется и это должно быть отображено на экране
    // поэтому вместо обычного List создаем ObservableCollection
    public ObservableCollection<Account> MineAccountsRoot { get; set; }
    public ObservableCollection<Account> ExternalAccountsRoot { get; set; }
    public ObservableCollection<Account> IncomesRoot { get; set; }
    public ObservableCollection<Account> ExpensesRoot { get; set; }

    public Visibility IsDeposit
    {
      get { return _isDeposit; }
      set
      {
        if (value.Equals(_isDeposit)) return;
        _isDeposit = value;
        NotifyOfPropertyChange(() => IsDeposit);
      }
    }

    public ObservableCollection<string> BalanceList { get; set; }

    public Account SelectedAccount
    {
      get { return _selectedAccount; }
      set
      {
        _selectedAccount = value;
        Period period = _openedAccountPage == 0 ? new Period(new DateTime(0), BalanceDate) : PaymentsPeriod;
        AccountBalanceInUsd = String.Format("{0:#,#} usd", _balanceCalculator.CountBalances(SelectedAccount, period, BalanceList));
        NotifyOfPropertyChange(() => SelectedAccount);
        IsDeposit = value.IsDescendantOf("Депозиты") && value.Children.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

      }
    }

    public int OpenedAccountPage
    {
      get { return _openedAccountPage; }
      set
      {
        _openedAccountPage = value;
        if (value == 0)
        {
          BalanceDateSelectControl = Visibility.Visible;
          PaymentsPeriodSelectControl = Visibility.Collapsed;
        }
        else
        {
          BalanceDateSelectControl = Visibility.Collapsed;
          PaymentsPeriodSelectControl = Visibility.Visible;
        }
        var a = FindSelectedOrAssignFirstAccountOnPage(_openedAccountPage);
        SelectedAccount = a;
      }
    }

    private Account GetSelectedInBranch(Account account)
    {
      if (account.IsSelected) return account;
      foreach (var child in account.Children)
      {
        var result = GetSelectedInBranch(child);
        if (result != null) return result;
      }
      return null;
    }

    private Account GetSelectedInCollection(IEnumerable<Account> roots)
    {
      foreach (var branch in roots)
      {
        var result = GetSelectedInBranch(branch);
        if (result != null) return result;
      }
      return null;
    }

    private Account FindSelectedOrAssignFirstAccountOnPage(int pageNumber)
    {
      ObservableCollection<Account> collection;
      switch (pageNumber)
      {
        case 0:
          collection = MineAccountsRoot; break;
        case 1:
          collection = ExternalAccountsRoot; break;
        case 2:
          collection = IncomesRoot; break;
        case 3:
          collection = ExpensesRoot; break;
        default:
          collection = MineAccountsRoot; break;
      }

      var result = GetSelectedInCollection(collection);

      if (result == null && collection.Count != 0)
      {
        result = (from account in collection
                  select account).First();
        result.IsSelected = true;
      }
      return result;
    }
    #endregion

    public ShellViewModel()
    {
      _message = "Keeper is running (On Debug)";
      BalanceList = new ObservableCollection<string> { "test balance" };
    }

    public void OnImportsSatisfied()
    {
      _isDbLoadingSuccessed = DbGeneralLoading.FullDbLoadProcess();

      InitVariablesToShowAccounts();
      InitBalanceControls();

      _balanceCalculator = new BalancesForShellCalculator(Db);
      _accountTreesFunctions = new AccountTreesFunctions(Db);
      _diagramDataCalculator = new DiagramDataExtractor(Db);
      _diagramDataCtor = new DiagramDataCtors(Db);
    }

    private void InitBalanceControls()
    {
      _balanceDate = DateTime.Today.AddDays(1).AddSeconds(-1);
      _paymentsPeriod = new Period(new DayProcessor(DateTime.Today).BeforeThisDay(), new DayProcessor(DateTime.Today).AfterThisDay());
      _isDbLoadingSuccessed = true;
    }

    private void InitVariablesToShowAccounts()
    {
      MineAccountsRoot = new ObservableCollection<Account>(from account in Db.Accounts
                                                           where account.Name == "Мои"
                                                           select account);
      ExternalAccountsRoot = new ObservableCollection<Account>(from account in Db.Accounts
                                                               where account.Name == "Внешние" || account.Name == "Для ввода стартовых остатков"
                                                               select account);
      IncomesRoot = new ObservableCollection<Account>(from account in Db.Accounts
                                                      where account.Name == "Все доходы"
                                                      select account);
      ExpensesRoot = new ObservableCollection<Account>(from account in Db.Accounts
                                                       where account.Name == "Все расходы"
                                                       select account);

      NotifyOfPropertyChange(() => MineAccountsRoot);
      NotifyOfPropertyChange(() => ExternalAccountsRoot);
      NotifyOfPropertyChange(() => IncomesRoot);
      NotifyOfPropertyChange(() => ExpensesRoot);
    }

    protected override void OnViewLoaded(object view)
    {
      if (!_isDbLoadingSuccessed)
      {
        TryClose();
        return;
      }
      DisplayName = "Keeper (c) 2012-13";
      Message = DateTime.Today.ToString("dddd , dd MMMM yyyy");
      OpenedAccountPage = 0;

//      if (!ShowLogonForm()) TryClose();

    }

    public override void CanClose(Action<bool> callback)
    {
      if (_isDbLoadingSuccessed)
      {
        foreach (var launchedForm in _launchedForms.Where(launchedForm => launchedForm.IsActive))
          launchedForm.TryClose();
        BinaryCrypto.DbCryptoSerialization(); // сериализует БД в dbx файл
        DbTxtSave.MakeDbBackupCopy(); // сохраняет резервную копию БД в текстовом виде , в шифрованный zip
      }
      callback(true);
    }

    #region // методы реализации контекстного меню на дереве счетов

    public void RemoveSelectedAccount()
    {
      _accountTreesFunctions.RemoveAccount(SelectedAccount);
    }

    public void AddSelectedAccount() 
    {
      _accountTreesFunctions.AddAccount(SelectedAccount);
      if (SelectedAccount.Name == "Депозиты") ReorderDepositAccounts();
    }

    private void ReorderDepositAccounts()
    {
      DbTxtSave.SaveDbInTxt();
      var result = DbTxtLoad.LoadDbFromTxt();
      if (result.Code != 0) MessageBox.Show(result.Explanation);
      else InitVariablesToShowAccounts();
    }

    public void ChangeSelectedAccount()
    {
      _accountTreesFunctions.ChangeAccount(SelectedAccount);
    }

    public List<DepositViewModel> LaunchedViewModels { get; set; }
    public void ShowDeposit()
    {
      if (!SelectedAccount.IsDescendantOf("Депозиты") || SelectedAccount.Children.Count != 0) return;

      foreach (var launchedForm in _launchedForms)
      {
        if (launchedForm is DepositViewModel && launchedForm.IsActive
            && ((DepositViewModel)launchedForm).Deposit.Account == SelectedAccount) launchedForm.TryClose();
      }

      var depositForm = new DepositViewModel(Db, SelectedAccount);
      _launchedForms.Add(depositForm);
      depositForm.Renewed += DepositViewModelRenewed; // ?
      WindowManager.ShowWindow(depositForm);
    }

    void DepositViewModelRenewed(object sender, Account newAccount)
    {
      SelectedAccount.IsSelected = false;
      SelectedAccount = newAccount;
    }

    #endregion

    #region // меню файл
    public void SaveDatabase()
    {
      BinaryCrypto.DbCryptoSerialization();
    }

    public void LoadDatabase()
    {
      var filename = Path.Combine(Settings.Default.SavePath, "Keeper.dbx");
      BinaryCrypto.DbCryptoDeserialization(filename);
    }

    public void ClearDatabase()
    {
      DbClear.ClearAllTables();

      IncomesRoot.Clear();
      ExpensesRoot.Clear();
      ExternalAccountsRoot.Clear();
      MineAccountsRoot.Clear();
    }

    public void MakeDatabaseBackup()
    {
      DbTxtSave.MakeDbBackupCopy();
    }

    public void ExportDatabaseToTxt()
    {
      DbTxtSave.SaveDbInTxt();
    }

    public void ImportDatabaseFromTxt()
    {
      var result = DbTxtLoad.LoadDbFromTxt();
      if (result.Code != 0) MessageBox.Show(result.Explanation);
      else InitVariablesToShowAccounts();
    }

    public void RemoveExtraBackups()
    {
      String arcMessage = Message;
      Message = "Удаление идентичных резервных копий";
      DbBackup.RemoveIdenticalBackups();
      Message = arcMessage;
      StatusBarItem0 = "Готово";
    }

    #endregion

    private readonly List<Screen> _launchedForms = new List<Screen>();

    #region // меню формы - вызовы дочерних окон

    public bool ShowLogonForm()
    {
      var logonViewModel = new LogonViewModel("1");
      WindowManager.ShowDialog(logonViewModel);
      return logonViewModel.Result;
    }

    public void ShowTransactionsForm()
    {
      var arcMessage = Message;
      Message = "Input operations";
      UsefulLists.FillLists();
      WindowManager.ShowDialog(new TransactionViewModel(Db));
      // по возвращении на главную форму пересчитать остаток/оборот по выделенному счету/категории
      var period = _openedAccountPage == 0 ? new Period(new DateTime(0), new DayProcessor(BalanceDate).AfterThisDay()) : PaymentsPeriod;
      _balanceCalculator.CountBalances(SelectedAccount, period, BalanceList);
      BinaryCrypto.DbCryptoSerialization();
      Message = arcMessage;
    }

    public void ShowCurrencyRatesForm()
    {
      var arcMessage = Message;
      Message = "Currency rates";
      WindowManager.ShowDialog(new RatesViewModel());
      Message = arcMessage;
    }

    public void ShowArticlesAssociationsForm()
    {
      var arcMessage = Message;
      Message = "Articles' associations";
      UsefulLists.FillLists();
      WindowManager.ShowDialog(new ArticlesAssociationsViewModel());
      Message = arcMessage;
    }

    public void ShowToDoForm()
    {
      var arcMessage = Message;
      Message = "TODO List";
      WindowManager.ShowDialog(new ToDoViewModel());
      Message = arcMessage;
    }

    public void ProgramExit()
    {
      TryClose();
    }

    public void ShowMonthAnalisysForm()
    {
      var arcMessage = Message;
      Message = "MonthAnalisys";
      WindowManager.ShowDialog(new MonthAnalisysViewModel(Db));
      Message = arcMessage;
    }

    public void ShowDepositsForm()
    {
      foreach (var launchedForm in _launchedForms)
        if (launchedForm is DepositViewModel && launchedForm.IsActive) launchedForm.TryClose();

      var depositsForm = new DepositsViewModel(Db);
      _launchedForms.Add(depositsForm);
      WindowManager.ShowWindow(depositsForm);
    }

    #endregion

    #region menu Diagrams

    public void ShowDailyBalancesDiagram()
    {
      var balances = _diagramDataCtor.DailyBalancesCtor();
      var diagramForm = new DiagramViewModel(balances);
      _launchedForms.Add(diagramForm);
      WindowManager.ShowWindow(diagramForm);
    }

    public void ShowRatesDiagram()
    {
      var rate = _diagramDataCtor.RatesCtor(CurrencyCodes.BYR);

      var diagramForm = new DiagramViewModel(rate);
      _launchedForms.Add(diagramForm);
      WindowManager.ShowWindow(diagramForm);
    }

    public void ShowMonthlyResultDiagram()
    {
      var monthlyResults = _diagramDataCtor.MonthlyResultsDiagramCtor();

      var barDiagramForm = new DiagramViewModel(monthlyResults);
      _launchedForms.Add(barDiagramForm);
      WindowManager.ShowWindow(barDiagramForm);
    }

    public void ShowMonthlyIncomeDiagram()
    {
      var monthlyIncomes = _diagramDataCtor.MonthlyIncomesDiagramCtor();

      var barDiagramForm = new DiagramViewModel(monthlyIncomes);
      _launchedForms.Add(barDiagramForm);
      WindowManager.ShowWindow(barDiagramForm);
    }

    public void ShowMonthlyOutcomeDiagram()
    {
      var monthlyOutcomes = _diagramDataCtor.MonthlyOutcomesDiagramCtor();

      var barDiagramForm = new DiagramViewModel(monthlyOutcomes);
      _launchedForms.Add(barDiagramForm);
      WindowManager.ShowWindow(barDiagramForm);
    }

    #endregion

    public void TempItem()
    {
      var balances = new DiagramData
                       {
                         Mode = DiagramMode.BarVertical,
                         TimeInterval = Every.Month,
                         Data = new List<DiagramSeries>{new DiagramSeries{
                           Data = new List<DiagramPair>
                                                                                   {
                                                                                     new DiagramPair(new DateTime(2002,1,1),300),
                                                                                     new DiagramPair(new DateTime(2012,1,1),400),
                                                                                     new DiagramPair(new DateTime(2012,2,1),200),
                                                                                   },
                                                                                   Index = 0,
                                                                                   Name = "bla",
                                                                                   PositiveBrushColor = Brushes.Blue
                         }
                         }

                       };

      var barDiagramForm = new DiagramViewModel(balances);
      _launchedForms.Add(barDiagramForm);
      WindowManager.ShowWindow(barDiagramForm);
    }

    #region date\period selection properties
    public DateTime BalanceDate
    {
      get { return _balanceDate; }
      set
      {
        if (value.Equals(_balanceDate)) return;
        _balanceDate = value.Date.AddDays(1).AddMilliseconds(-1);
        NotifyOfPropertyChange(() => BalanceDate);
        var period = new Period(new DateTime(0), new DayProcessor(BalanceDate).AfterThisDay());
        AccountBalanceInUsd = String.Format("{0:#,#} usd", _balanceCalculator.CountBalances(SelectedAccount, period, BalanceList));
      }
    }

    public Period PaymentsPeriod
    {
      get { return _paymentsPeriod; }
      set
      {
        if (Equals(value, _paymentsPeriod)) return;
        _paymentsPeriod = value;
        NotifyOfPropertyChange(() => PaymentsPeriod);
        AccountBalanceInUsd = string.Format("{0:#,#} usd",
                                            _balanceCalculator.CountBalances(SelectedAccount, _paymentsPeriod, BalanceList));
      }
    }

    public Visibility BalanceDateSelectControl
    {
      get { return _balanceDateSelectControl; }
      set
      {
        if (Equals(value, _balanceDateSelectControl)) return;
        _balanceDateSelectControl = value;
        NotifyOfPropertyChange(() => BalanceDateSelectControl);
      }
    }

    public Visibility PaymentsPeriodSelectControl
    {
      get { return _paymentsPeriodSelectControl; }
      set
      {
        if (Equals(value, _paymentsPeriodSelectControl)) return;
        _paymentsPeriodSelectControl = value;
        NotifyOfPropertyChange(() => PaymentsPeriodSelectControl);
      }
    }

    #endregion
  }
}
