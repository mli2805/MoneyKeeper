using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Soap;
using System.Windows;
using System.Xml.Serialization;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Properties;
using Keeper.Utils;


namespace Keeper.ViewModels
{
  [Export(typeof(IShell))] // это для загрузчика, который ищет главное окно проги
  [Export(typeof(ShellViewModel)), PartCreationPolicy(CreationPolicy.Shared)] // это для класса Account чтобы засунуть в свойство SelectedAccount 
  public class ShellViewModel : Screen, IShell, IPartImportsSatisfiedNotification
  {
    [Import]
    public IWindowManager WindowManager { get; set; }

    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    #region // поля/свойства в классе Модели к которым биндятся визуальные элементы из Вью

    // чисто по приколу, label на вьюхе, которая по ходу программы может меняться - поэтому свойство с нотификацией
    private string _message;
    private string _statusBarItem0;
    private Account _selectedAccount;
    private int _openedAccountPage;
    private DateTime _balanceDate;
    private Visibility _balancePeriodChoiseControls;
    private Visibility _paymentsPeriodChoiseControls;
    private DateTime _paymentsStartDate;
    private DateTime _paymentsFinishDate;
    private string _accountBalanceInUsd;
    private Visibility _isDeposit;
    private bool _isDbLoadingSuccessed;

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
        Period period = _openedAccountPage == 0 ? new Period(new DateTime(0), BalanceDate) : new Period(PaymentsStartDate, PaymentsFinishDate);
        AccountBalanceInUsd = String.Format("{0:#,#} usd", Balance.CountBalances(SelectedAccount, period, BalanceList));
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
          BalancePeriodChoiseControls = Visibility.Visible;
          PaymentsPeriodChoiseControls = Visibility.Collapsed;
        }
        else
        {
          BalancePeriodChoiseControls = Visibility.Collapsed;
          PaymentsPeriodChoiseControls = Visibility.Visible;
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

    private Account GetSelectedInCollection(ObservableCollection<Account> collection)
    {
      foreach (var branch in collection)
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
      _depositsFormPointer = null;
    }

    public void OnImportsSatisfied()
    {
      _isDbLoadingSuccessed = false;
      if (BinaryCrypto.DbCryptoDeserialization() == 5)
      {
        var filename = Path.Combine(Settings.Default.SavePath, "Keeper.dbx");
        MessageBox.Show("");
        MessageBox.Show("File '" + filename + "' not found. \n Last zip will be used.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);

        var loadResult = DbTxtLoad.LoadFromLastZip();
        if (loadResult.Code != 0)
        {
          MessageBox.Show(loadResult.Explanation + ". \n Application will be closed!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
          return;
        }

      }

      InitVariablesToShowAccounts();
      _balanceDate = DateTime.Today.AddDays(1).AddSeconds(-1);
      _paymentsStartDate = DateTime.Today.AddDays(-DateTime.Today.Day + 1);
      _paymentsFinishDate = DateTime.Today.AddDays(1).AddSeconds(-1);
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
    }

    public override void CanClose(Action<bool> callback)
    {
      if (_isDbLoadingSuccessed)
      {
        if (_ratesDiagramFormPointer != null && _ratesDiagramFormPointer.IsActive) _ratesDiagramFormPointer.TryClose();
        if (_depositsFormPointer != null && _depositsFormPointer.IsActive) _depositsFormPointer.TryClose();
        if (LaunchedViewModels != null)
          foreach (var depositViewModel in LaunchedViewModels)
            if (depositViewModel.IsActive) depositViewModel.TryClose();

        BinaryCrypto.DbCryptoSerialization(); // сериализует БД в dbx файл
        DbTxtSave.MakeDbBackupCopy(); // сохраняет резервную копию БД в текстовом виде , в шифрованный zip
      }
      callback(true);
    }

    #region // методы реализации контекстного меню на дереве счетов

    public void RemoveAccount()
    {
      if (SelectedAccount.Parent == null)
      {
        MessageBox.Show("Корневой счет нельзя удалять!", "Отказ!");
        return;
      }
      if (SelectedAccount.Children.Count > 0)
      {
        MessageBox.Show("Удалять разрешено \n только конечные листья дерева счетов!", "Отказ!");
        return;
      }
      // такой запрос возвращает не коллекцию, а энумератор
      IEnumerable<Transaction> tr = from transaction in Db.Transactions
                                    where transaction.Debet == SelectedAccount || transaction.Credit == SelectedAccount || transaction.Article == SelectedAccount
                                    select transaction;

      // Any() пытается двинуться по этому энумератору и если может, то true
      if (tr.Any())
      {
        MessageBox.Show("Этот счет используется в проводках!", "Отказ!");
        return;
      }
      if (MessageBox.Show("Проверено, счет не используется в транзакциях.\n Удаление счета\n\n <<" + SelectedAccount.Name + ">>\n          Удалить?", "Confirm",
                          MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

      Db.AccountsPlaneList.Remove(SelectedAccount);
      SelectedAccount.Parent.Children.Remove(SelectedAccount);
    }

    public void AddAccount()
    {
      var accountInWork = new Account { Parent = SelectedAccount };
      if (WindowManager.ShowDialog(new AddAndEditAccountViewModel(accountInWork, "Добавить")) != true) return;

      SelectedAccount = accountInWork.Parent;
      accountInWork.Id = (from account in Db.AccountsPlaneList select account.Id).Max() + 1;
      SelectedAccount.Children.Add(accountInWork);

      if (SelectedAccount.Name == "Депозиты")
      {
          DbTxtSave.SaveDbInTxt();
          var result = DbTxtLoad.LoadDbFromTxt();
          if (result.Code != 0) MessageBox.Show(result.Explanation);
          else InitVariablesToShowAccounts();
      }

      Db.AccountsPlaneList.Clear();
      Db.AccountsPlaneList = KeeperDb.FillInAccountsPlaneList(Db.Accounts);
      UsefulLists.FillLists();
    }

    public void ChangeAccount()
    {
      var accountInWork = new Account();
      Account.CopyForEdit(accountInWork, SelectedAccount);
      if (WindowManager.ShowDialog(new AddAndEditAccountViewModel(accountInWork, "Редактировать")) != true) return;

      if (SelectedAccount.Parent != accountInWork.Parent)
      {
        accountInWork.Parent.Children.Add(accountInWork);
        SelectedAccount.Parent.Children.Remove(SelectedAccount);
      }
      else SelectedAccount.Name = accountInWork.Name;
      //      Account.CopyForEdit(SelectedAccount, accountInWork);
    }

    public List<DepositViewModel> LaunchedViewModels { get; set; }
    public void ShowDeposit()
    {
      if (SelectedAccount.IsDescendantOf("Депозиты") && SelectedAccount.Children.Count == 0)
      {
        if (LaunchedViewModels == null) LaunchedViewModels = new List<DepositViewModel>();
        else
        {
          var depositView = (from d in LaunchedViewModels
                             where d.Deposit.Account == SelectedAccount
                             select d).FirstOrDefault();
          if (depositView != null) depositView.TryClose();
        }
        var depositViewModel = new DepositViewModel(SelectedAccount);
        LaunchedViewModels.Add(depositViewModel);
        depositViewModel.Renewed += DepositViewModelRenewed;
        WindowManager.ShowWindow(depositViewModel);
      }
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
      BinaryCrypto.DbCryptoDeserialization();
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

    #region // меню формы - вызовы дочерних окон

    public void ShowTransactionsForm()
    {
      String arcMessage = Message;
      Message = "Input operations";
      UsefulLists.FillLists();
      WindowManager.ShowDialog(new TransactionViewModel());
      // по возвращении на главную форму пересчитать остаток/оборот по выделенному счету/категории
      Period period = _openedAccountPage == 0 ? new Period(new DateTime(0), BalanceDate) : new Period(PaymentsStartDate, PaymentsFinishDate);
      Balance.CountBalances(SelectedAccount, period, BalanceList);
      BinaryCrypto.DbCryptoSerialization();
      Message = arcMessage;
    }

    public void ShowCurrencyRatesForm()
    {
      String arcMessage = Message;
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
      WindowManager.ShowDialog(new MonthAnalisysViewModel());
      Message = arcMessage;
    }

    private DepositsViewModel _depositsFormPointer;
    public void ShowDepositsForm()
    {
      if (_depositsFormPointer != null && _depositsFormPointer.IsActive) _depositsFormPointer.TryClose();
      _depositsFormPointer = new DepositsViewModel();
      WindowManager.ShowWindow(_depositsFormPointer);
    }

    #endregion

    #region menu Diagrams

    private RatesDiagramViewModel _ratesDiagramFormPointer;
    public void ShowDailyBalancesDiagram()
    {
      var allMyMoney = (from account in Db.Accounts where account.Name == "Мои" select account).FirstOrDefault();
      var balances = DiagramDataCtors.AccountBalancesForPeriodInUsdThirdWay(allMyMoney, new Period(new DateTime(2001, 12, 31), DateTime.Today), Every.Day);

      _ratesDiagramFormPointer = new RatesDiagramViewModel(balances);
      WindowManager.ShowWindow(_ratesDiagramFormPointer);
    }

    public void ShowRatesDiagram()
    {
//      var rates = Db.CurrencyRates.Where(r => r.Currency == CurrencyCodes.EUR).OrderBy(r => r.BankDay).
//                           ToDictionary(currencyRate => currencyRate.BankDay, currencyRate => (decimal)(1 / currencyRate.Rate));
      var rates = Db.CurrencyRates.Where(r => r.Currency == CurrencyCodes.BYR).OrderBy(r => r.BankDay).
                           ToDictionary(currencyRate => currencyRate.BankDay, currencyRate => (decimal)currencyRate.Rate);

      _ratesDiagramFormPointer = new RatesDiagramViewModel(rates);
      WindowManager.ShowWindow(_ratesDiagramFormPointer);
    }

    private MonthlyResultDiagramViewModel _monthlyResultDiagramFormPointer;
    public void ShowMonthlyResultDiagram()
    {
      var monthlyResults = DiagramDataCtors.MonthlyResultsDiagramCtor();

      _monthlyResultDiagramFormPointer = new MonthlyResultDiagramViewModel(monthlyResults,BarDiagramMode.Butterfly);
      WindowManager.ShowWindow(_monthlyResultDiagramFormPointer);
    }

    public void ShowMonthlyIncomeDiagram()
    {
      var monthlyIncomes = DiagramDataCtors.MonthlyIncomesDiagramCtor();

      _monthlyResultDiagramFormPointer = new MonthlyResultDiagramViewModel(monthlyIncomes,BarDiagramMode.Vertical);
      WindowManager.ShowWindow(_monthlyResultDiagramFormPointer);
    }

    #endregion

    public void TempItem()
    {
      var ratesByrUsd = Db.CurrencyRates.Where(r => r.Currency == CurrencyCodes.BYR).OrderBy(r => r.BankDay).
                           ToDictionary(currencyRate => currencyRate.BankDay, currencyRate => (decimal)currencyRate.Rate);

      var rates2 = new Dictionary<DateTime, decimal>();
      foreach (var pair in ratesByrUsd)
      {
        var days = (pair.Key - new DateTime(0)).TotalDays;
        if ((days%2).Equals(0)) rates2.Add(pair.Key.AddDays(-193),(decimal)1.1*pair.Value);
        else rates2.Add(pair.Key.AddDays(-193), (decimal)0.9 * pair.Value);
      }

      var rates3 = new Dictionary<DateTime, decimal>();
      foreach (var pair in ratesByrUsd)
      {
        rates3.Add(pair.Key.AddDays(78), (decimal)Math.Log10((double)pair.Value));
      }

      var rates = new List<Dictionary<DateTime, decimal>>();
      rates.Add(ratesByrUsd);
      rates.Add(rates2);
      rates.Add(rates3);
      WindowManager.ShowWindow(new DateDoubleDiagramViewModel(rates,0));
    }

    // методы привязанные к группам контролов выбора даты, на которую остатки (дат, между которыми обороты)
    // свойства куда эти даты заносятся, свойства видимости этих групп контролов
    #region
    public DateTime BalanceDate
    {
      get { return _balanceDate; }
      set
      {
        if (value.Equals(_balanceDate)) return;
        _balanceDate = value.Date.AddDays(1).AddMilliseconds(-1);
        NotifyOfPropertyChange(() => BalanceDate);
        var period = new Period(new DateTime(0), BalanceDate);
        AccountBalanceInUsd = String.Format("{0:#,#} usd", Balance.CountBalances(SelectedAccount, period, BalanceList));
      }
    }

    public void TodayBalance() { BalanceDate = DateTime.Today; }
    public void YesterdayBalance() { BalanceDate = DateTime.Today.AddDays(-1); }
    public void LastMonthEndBalance() { BalanceDate = DateTime.Today.AddDays(-DateTime.Today.Day + 1); }

    public void OneDayBeforeBalance() { BalanceDate = BalanceDate.AddDays(-1); }
    public void OneMonthBeforeBalance() { BalanceDate = BalanceDate.AddMonths(-1); }
    public void OneYearBeforeBalance() { BalanceDate = BalanceDate.AddYears(-1); }

    public void OneDayAfterBalance() { BalanceDate = BalanceDate.AddDays(1); }
    public void OneMonthAfterBalance() { BalanceDate = BalanceDate.AddMonths(1); }
    public void OneYearAfterBalance() { BalanceDate = BalanceDate.AddYears(1); }

    public DateTime PaymentsStartDate
    {
      get { return _paymentsStartDate; }
      set
      {
        if (value.Equals(_paymentsStartDate)) return;
        _paymentsStartDate = value;
        NotifyOfPropertyChange(() => PaymentsStartDate);
        var period = new Period(PaymentsStartDate, PaymentsFinishDate);
        AccountBalanceInUsd = String.Format("{0:#,#} usd", Balance.CountBalances(SelectedAccount, period, BalanceList));
      }
    }

    public DateTime PaymentsFinishDate
    {
      get { return _paymentsFinishDate; }
      set
      {
        if (value.Equals(_paymentsFinishDate)) return;
        _paymentsFinishDate = value.Date.AddDays(1).AddMilliseconds(-1);
        NotifyOfPropertyChange(() => PaymentsFinishDate);
        var period = new Period(PaymentsStartDate, PaymentsFinishDate);
        AccountBalanceInUsd = String.Format("{0:#,#} usd", Balance.CountBalances(SelectedAccount, period, BalanceList));
      }
    }

    public void TodayPayments() { PaymentsStartDate = DateTime.Today; PaymentsFinishDate = DateTime.Today; }
    public void YesterdayPayments() { PaymentsStartDate = DateTime.Today.AddDays(-1); PaymentsFinishDate = DateTime.Today.AddDays(-1); }
    public void ThisMonthPayments()
    {
      PaymentsStartDate = DateTime.Today.AddDays(-DateTime.Today.Day + 1);
      PaymentsFinishDate = DateTime.Today;
    }
    public void LastMonthPayments()
    {
      PaymentsFinishDate = DateTime.Today.AddDays(-DateTime.Today.Day);
      PaymentsStartDate = PaymentsFinishDate.AddDays(-PaymentsFinishDate.Day + 1);
    }
    public void ThisYearPayments()
    {
      PaymentsStartDate = DateTime.Today.AddDays(-DateTime.Today.DayOfYear + 1);
      PaymentsFinishDate = DateTime.Today;
    }
    public void LastYearPayments()
    {
      PaymentsFinishDate = DateTime.Today.AddDays(-DateTime.Today.DayOfYear);
      PaymentsStartDate = PaymentsFinishDate.AddDays(-PaymentsFinishDate.DayOfYear + 1);
    }
    public void OneDayBeforePayments() { PaymentsStartDate = PaymentsStartDate.AddDays(-1); PaymentsFinishDate = PaymentsFinishDate.AddDays(-1); }
    public void OneMonthBeforePayments()
    {
      PaymentsStartDate = PaymentsStartDate.AddMonths(-1);
      PaymentsFinishDate = IsLastDayOfMonth(PaymentsFinishDate) ? PaymentsFinishDate.AddDays(-PaymentsFinishDate.Day) : PaymentsFinishDate.AddMonths(-1);
    }

    public void OneYearBeforePayments() { PaymentsStartDate = PaymentsStartDate.AddYears(-1); PaymentsFinishDate = PaymentsFinishDate.AddYears(-1); }
    public void OneDayAfterPayments() { PaymentsStartDate = PaymentsStartDate.AddDays(1); PaymentsFinishDate = PaymentsFinishDate.AddDays(1); }
    public void OneMonthAfterPayments()
    {
      PaymentsStartDate = PaymentsStartDate.AddMonths(1);
      if (IsLastDayOfMonth(PaymentsFinishDate))
      {
        PaymentsFinishDate = PaymentsFinishDate.AddMonths(2);
        PaymentsFinishDate = PaymentsFinishDate.AddDays(-PaymentsFinishDate.Day);
      }
      else PaymentsFinishDate = PaymentsFinishDate.AddMonths(1);
    }

    public void OneYearAfterPayments() { PaymentsStartDate = PaymentsStartDate.AddYears(1); PaymentsFinishDate = PaymentsFinishDate.AddYears(1); }

    public Visibility BalancePeriodChoiseControls
    {
      get { return _balancePeriodChoiseControls; }
      set
      {
        if (Equals(value, _balancePeriodChoiseControls)) return;
        _balancePeriodChoiseControls = value;
        NotifyOfPropertyChange(() => BalancePeriodChoiseControls);
      }
    }

    public Visibility PaymentsPeriodChoiseControls
    {
      get { return _paymentsPeriodChoiseControls; }
      set
      {
        if (Equals(value, _paymentsPeriodChoiseControls)) return;
        _paymentsPeriodChoiseControls = value;
        NotifyOfPropertyChange(() => PaymentsPeriodChoiseControls);
      }
    }

    private bool IsLastDayOfMonth(DateTime date) { return date.Month != date.AddDays(1).Month; }

    #endregion

    #region SOAP - не обрабатывает дженерики

    private static void DbSoapSerialization()
    {
      var watch1 = new Stopwatch();
      watch1.Start();

      var soapFormatter = new SoapFormatter();
      using (Stream fStream = new FileStream("CurrencyRates.soap", FileMode.Create, FileAccess.Write))
      {
        foreach (var currencyRate in Db.CurrencyRates)
        {
          soapFormatter.Serialize(fStream, currencyRate);
        }
      }

      watch1.Stop();
      Console.WriteLine("SoapFormatter serialization time is {0}", watch1.Elapsed);
    }

    private static void DbSoapDeserialization()
    {
      var watch1 = new Stopwatch();
      watch1.Start();

      var rates = new ObservableCollection<CurrencyRate>();
      var soapFormatter = new SoapFormatter();
      using (Stream fStream = new FileStream("CurrencyRates.soap", FileMode.Open, FileAccess.Read))
      {
        //  как цикл по файлу устроить      ??????????????????????????
        var rate1 = (CurrencyRate)soapFormatter.Deserialize(fStream);
        rates.Add(rate1); 
      }

      watch1.Stop();
      Console.WriteLine("SoapFormatter deserialization time is {0}", watch1.Elapsed);
    }

    #endregion

    #region XML serialization
    // дженерик проглотила нормально
    // сломалась на дереве счетов - Account содержит Account

    private static void DbXmlSerialization()
    {
      var watch1 = new Stopwatch();
      watch1.Start();

      var xmlSerializer = new XmlSerializer(typeof(ObservableCollection<CurrencyRate>));
      using (Stream fStream = new FileStream("CurrencyRates.xml", FileMode.Create, FileAccess.Write))
      {
          xmlSerializer.Serialize(fStream, Db.CurrencyRates);
      }

      watch1.Stop();
      Console.WriteLine("XmlSerializer serialization time is {0}", watch1.Elapsed);
    }

    private static void DbXmlDeserialization()
    {
      var watch1 = new Stopwatch();
      watch1.Start();

      var db1 = new ObservableCollection<CurrencyRate>();
      var xmlSerializer = new XmlSerializer(typeof(ObservableCollection<CurrencyRate>));
      using (Stream fStream = new FileStream("CurrencyRates.xml", FileMode.Open, FileAccess.Read))
      {
        db1 = (ObservableCollection<CurrencyRate>)xmlSerializer.Deserialize(fStream);
      }

      watch1.Stop();
      Console.WriteLine("XmlSerializer deserialization time is {0}", watch1.Elapsed);
    }

    #endregion
  }
}
