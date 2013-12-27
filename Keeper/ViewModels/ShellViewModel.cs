using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Properties;
using Keeper.Utils;
using Keeper.Utils.Accounts;
using Keeper.Utils.Balances;
using Keeper.Utils.DbInputOutput;
using Keeper.Utils.DbInputOutput.CompositeTasks;
using Keeper.Utils.DbInputOutput.TxtTasks;
using Keeper.Utils.Diagram;


namespace Keeper.ViewModels
{
  [Export(typeof(IShell))] // это для загрузчика, который ищет главное окно проги
  [Export(typeof(ShellViewModel))]
  [Shared] // это для класса Account чтобы засунуть в свойство SelectedAccount 
  public class ShellViewModel : Screen, IShell
  {
    [Import]
    public IWindowManager WindowManager { get; set; }
    private readonly BackgroundWorker _backgroundWorker;
    private bool _isBackgroundWorkerBusy;

    //    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }
    public KeeperDb Db;
    readonly DbLoadResult mLoadResult;

    private readonly AccountTreesGardener _accountTreesGardener;
    private readonly AccountInTreeSeeker _accountInTreeSeeker;
    private readonly DbToTxtSaver _txtSaver;
    private readonly DbBackuper _backuper;
    private readonly BalancesForShellCalculator _balanceCalculator;
    private readonly DiagramDataCtors _diagramDataCtor;

    #region // поля/свойства в классе Модели к которым биндятся визуальные элементы из Вью

    // чисто по приколу, label на вьюхе, которая по ходу программы может меняться - поэтому свойство с нотификацией
    private string _message;
    private string _statusBarItem0;
    private Visibility _isProgressBarVisible;
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

    public Visibility IsProgressBarVisible
    {
      get { return _isProgressBarVisible; }
      set
      {
        if (Equals(value, _isProgressBarVisible)) return;
        _isProgressBarVisible = value;
        NotifyOfPropertyChange(() => IsProgressBarVisible);
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

    [ImportingConstructor]
    public ShellViewModel(KeeperDb db, DbLoadResult loadResult, BalancesForShellCalculator balancesForShellCalculator,
      AccountInTreeSeeker accountInTreeSeeker, DbToTxtSaver txtSaver, DbBackuper backuper)
    {
      Db = db;
      mLoadResult = loadResult;

      _isDbLoadingSuccessed = Db != null;
      if (!_isDbLoadingSuccessed)
      {
        MessageBox.Show(mLoadResult.Explanation + "\nApplication will be closed!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      _backgroundWorker = new BackgroundWorker();
      _backgroundWorker.WorkerReportsProgress = true;
      _backgroundWorker.WorkerSupportsCancellation = true;
      _backgroundWorker.DoWork += BackgroundWorkerDoWork;
      _backgroundWorker.RunWorkerCompleted += BackgroundWorkCompleted;
      _isBackgroundWorkerBusy = false;
      StatusBarItem0 = "Idle";
      IsProgressBarVisible = Visibility.Collapsed;

      _accountTreesGardener = new AccountTreesGardener(Db);
      InitVariablesToShowAccounts();
      InitBalanceControls();

      _balanceCalculator = balancesForShellCalculator;
      _accountInTreeSeeker = accountInTreeSeeker;
      _txtSaver = txtSaver;
      _backuper = backuper;
      _diagramDataCtor = new DiagramDataCtors(Db, _accountInTreeSeeker);
    }

    public void LaunchLongTaskInBackground(int number)
    {
      while (_isBackgroundWorkerBusy) { }
      _isBackgroundWorkerBusy = true;
      switch (number)
      {
        case 1:
          StatusBarItem0 = "Сохранение данных на диск";
          break;
        case 2:
          StatusBarItem0 = "Создание резервной копии БД";
          break;
        case 3:
          StatusBarItem0 = "Подготовка данных для диаграммы расходов";
          break;
      }
      IsProgressBarVisible = Visibility.Visible;
      _backgroundWorker.RunWorkerAsync(number);
    }

    // весь этот метод выполняется в другом потоке! 
    // не обращаться к разделяемым данным (таким как поля класса окна) или объектам пользовательского интерфейса.
    private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
    {
      switch ((int)e.Argument)
      {
        case 1: new DbSerializer().EncryptAndSerialize(Db, Path.Combine(Settings.Default.DbPath, Settings.Default.DbxFile));
          break;
        case 2: _backuper.MakeDbBackupCopy(); // сохраняет резервную копию БД в текстовом виде , в шифрованный zip
          break;
        case 3: e.Result = _diagramDataCtor.MonthlyOutcomesDiagramCtor();
          break;
      }
    }

    private RunWorkerCompletedEventArgs _backgroundWorkerResult;
    private void BackgroundWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      _backgroundWorkerResult = e;
      StatusBarItem0 = "Idle";
      IsProgressBarVisible = Visibility.Collapsed;
      _isBackgroundWorkerBusy = false;
    }

    private void InitBalanceControls()
    {
      _balanceDate = DateTime.Today.AddDays(1).AddSeconds(-1);
      _paymentsPeriod = new Period(new DayProcessor(DateTime.Today).BeforeThisDay(), new DayProcessor(DateTime.Today).AfterThisDay());
      BalanceList = new ObservableCollection<string>();
      _isDbLoadingSuccessed = true;
    }

    public void InitVariablesToShowAccounts()
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
        SerializeWithProgressBar();
        MakeBackupWithProgressBar();
        _task1.Wait();
      }
      callback(true);
    }

    #region // методы реализации контекстного меню на дереве счетов

    public void RemoveSelectedAccount()
    {
      _accountTreesGardener.RemoveAccount(SelectedAccount);
    }

    public void AddSelectedAccount()
    {
      _accountTreesGardener.AddAccount(SelectedAccount);
      if (SelectedAccount.Name == "Депозиты") ReorderDepositAccounts();
    }

    private void ReorderDepositAccounts()
    {
      _txtSaver.SaveDbInTxt();
      var result = new DbFromTxtLoader().LoadDbFromTxt(Settings.Default.TemporaryTxtDbPath);
      if (result.Code != 0) MessageBox.Show(result.Explanation);
      else
      {
        Db = result.Db;
        //        InitVariablesToShowAccounts();
      }
    }

    public void ChangeSelectedAccount()
    {
      _accountTreesGardener.ChangeAccount(SelectedAccount);
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

      var depositForm = IoC.Get<DepositViewModel>();
      depositForm.SetAccount(SelectedAccount);
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
      new DbSerializer().EncryptAndSerialize(Db, Path.Combine(Settings.Default.DbPath, Settings.Default.DbxFile));
    }

    public void LoadDatabase()
    {
      Db = new DbSerializer().DecryptAndDeserialize(Path.Combine(Settings.Default.DbPath, Settings.Default.DbxFile));
    }

    public void ClearDatabase()
    {
      new DbCleaner().ClearAllTables(Db);

      IncomesRoot.Clear();
      ExpensesRoot.Clear();
      ExternalAccountsRoot.Clear();
      MineAccountsRoot.Clear();
    }

    public void MakeDatabaseBackup()
    {
      _backuper.MakeDbBackupCopy();
    }

    public void ExportDatabaseToTxt()
    {
      _txtSaver.SaveDbInTxt();
    }

    public void ImportDatabaseFromTxt()
    {
      var result = new DbFromTxtLoader().LoadDbFromTxt(Settings.Default.TemporaryTxtDbPath);
      if (result.Code != 0) MessageBox.Show(result.Explanation);
      else
      {
        Db = result.Db;
        InitVariablesToShowAccounts();
      }
    }

    public void RemoveExtraBackups()
    {
      String arcMessage = Message;
      Message = "Удаление идентичных резервных копий";
      new DbBackupOrganizer().RemoveIdenticalBackups();
      Message = arcMessage;
      StatusBarItem0 = "Готово";
    }

    #endregion

    private readonly List<Screen> _launchedForms = new List<Screen>();

    private async void SerializeWithProgressBar()
    {
      StatusBarItem0 = "Сохранение данных на диск";
      IsProgressBarVisible = Visibility.Visible;
      await
        Task.Run(() => new DbSerializer().EncryptAndSerialize(Db, Path.Combine(Settings.Default.DbPath, Settings.Default.DbxFile)));
      StatusBarItem0 = "Idle";
      IsProgressBarVisible = Visibility.Collapsed;
    }

    void MakeBackupWithProgressBarTask()
    {
      _backuper.MakeDbBackupCopy();
    }

    // сохраняет резервную копию БД в текстовом виде , в шифрованный zip
    private Task _task1;
    private void MakeBackupWithProgressBar()
    {
      StatusBarItem0 = "Создание резервной копии БД";
      IsProgressBarVisible = Visibility.Visible;
      
      _task1 = new Task(MakeBackupWithProgressBarTask);
      _task1.Start();

      StatusBarItem0 = "Idle";
      IsProgressBarVisible = Visibility.Collapsed;
    }

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
      UsefulLists.FillLists(Db);
      WindowManager.ShowDialog(IoC.Get<TransactionViewModel>());
      // по возвращении на главную форму пересчитать остаток/оборот по выделенному счету/категории
      var period = _openedAccountPage == 0 ? new Period(new DateTime(0), new DayProcessor(BalanceDate).AfterThisDay()) : PaymentsPeriod;
      _balanceCalculator.CountBalances(SelectedAccount, period, BalanceList);
      SerializeWithProgressBar();
      if (OpenedAccountPage == 0) BalanceDate = BalanceDate; else PaymentsPeriod = PaymentsPeriod;
      Message = arcMessage;
    }

    public void ShowCurrencyRatesForm()
    {
      var arcMessage = Message;
      Message = "Currency rates";
      UsefulLists.FillLists(Db);
      WindowManager.ShowDialog(new RatesViewModel(Db));
      SerializeWithProgressBar();
      if (OpenedAccountPage == 0) BalanceDate = BalanceDate; else PaymentsPeriod = PaymentsPeriod;
      Message = arcMessage;
    }

    public void ShowArticlesAssociationsForm()
    {
      var arcMessage = Message;
      Message = "Articles' associations";
      UsefulLists.FillLists(Db);
      WindowManager.ShowDialog(new ArticlesAssociationsViewModel(Db));
      Message = arcMessage;
    }

    public void ShowToDoForm()
    {
      var toDoForm = new ToDoViewModel();
      _launchedForms.Add(toDoForm);
      WindowManager.ShowWindow(toDoForm);
    }

    public void ProgramExit()
    {
      TryClose();
    }

    public void ShowMonthAnalisysForm()
    {
      var arcMessage = Message;
      Message = "MonthAnalisys";
      WindowManager.ShowDialog(IoC.Get<MonthAnalisysViewModel>());
      Message = arcMessage;
    }

    public void ShowDepositsForm()
    {
      foreach (var launchedForm in _launchedForms)
        if (launchedForm is DepositViewModel && launchedForm.IsActive) launchedForm.TryClose();

      var depositsForm = IoC.Get<DepositsViewModel>();

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
      var rate = _diagramDataCtor.RatesCtor();

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

      var diagramForm = new DiagramViewModel(monthlyIncomes);
      _launchedForms.Add(diagramForm);
      WindowManager.ShowWindow(diagramForm);
    }

    public void ShowMonthlyOutcomeDiagram()
    {
      var monthlyOutcomes = _diagramDataCtor.MonthlyOutcomesDiagramCtor();

      var diagramForm = new DiagramViewModel(monthlyOutcomes);
      _launchedForms.Add(diagramForm);
      WindowManager.ShowWindow(diagramForm);
    }

    public void ShowAverageSignificancesDiagram()
    {
      var averageSignificances = _diagramDataCtor.AverageSignificancesDiagramCtor();

      var diagramForm = new DiagramViewModel(averageSignificances);
      _launchedForms.Add(diagramForm);
      WindowManager.ShowWindow(diagramForm);
    }
    #endregion

    public void TempItem()
    {
    }

    #region date\period selection properties
    public DateTime BalanceDate
    {
      get { return _balanceDate; }
      set
      {
        _balanceDate = new DayProcessor(value.Date).AfterThisDay();
        var period = new Period(new DateTime(0), new DayProcessor(BalanceDate).AfterThisDay());
        AccountBalanceInUsd = String.Format("{0:#,#} usd",
          _balanceCalculator.CountBalances(SelectedAccount, period, BalanceList));
      }
    }

    public Period PaymentsPeriod
    {
      get { return _paymentsPeriod; }
      set
      {
        _paymentsPeriod = value;
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
