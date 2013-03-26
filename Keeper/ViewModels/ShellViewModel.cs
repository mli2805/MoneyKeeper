using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Security.Cryptography;
using System.Windows;
using System.Xml.Serialization;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils;


namespace Keeper.ViewModels
{
  [Export(typeof(IShell))] // это для загрузчика, который ищет главное окно проги
  [Export(typeof(ShellViewModel)), PartCreationPolicy(CreationPolicy.Shared)] // это для класса Account чтобы засунуть в свойство SelectedAccount 
  public class ShellViewModel : Screen, IShell, IPartImportsSatisfiedNotification
  {
    [Import]
    public IWindowManager WindowManager { get; set; }

    public static KeeperTxtDb Db { get { return IoC.Get<KeeperTxtDb>(); } }
    public DepositsViewModel DepositsFormPointer { get; set; }

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

    // во ViewModel создается public property к которому будет биндиться компонент из View
    // далее содержимое этого свойства изменяется и это должно быть отображено на экране
    // поэтому вместо обычного List создаем ObservableCollection
    public ObservableCollection<Account> MineAccountsRoot { get; set; }
    public ObservableCollection<Account> ExternalAccountsRoot { get; set; }
    public ObservableCollection<Account> IncomesRoot { get; set; }
    public ObservableCollection<Account> ExpensesRoot { get; set; }

    public ObservableCollection<string> BalanceList { get; set; }

    public Account SelectedAccount
    {
      get { return _selectedAccount; }
      set
      {
        _selectedAccount = value;
        Period period = _openedAccountPage == 0 ? new Period(new DateTime(0), BalanceDate) : new Period(PaymentsStartDate, PaymentsFinishDate);
        Balance.CountBalances(SelectedAccount, period, BalanceList);
        NotifyOfPropertyChange(() => SelectedAccount);
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

    public DbLoadError DbLoadResult { get; set; }

    public ShellViewModel()
    {
      _message = "Keeper is running (On Debug)";
      //      Database.SetInitializer(new DbInitializer());
      BalanceList = new ObservableCollection<string> { "test balance" };
      DepositsFormPointer = null;
    }

    public void OnImportsSatisfied()
    {
      TimeSpan elapsed;
      DbLoadResult = DbLoad.LoadAllTables(out elapsed);
      StatusBarItem0 = elapsed.ToString();

      InitVariablesToShowAccounts();
      _balanceDate = DateTime.Today.AddDays(1).AddSeconds(-1);
      _paymentsStartDate = DateTime.Today.AddDays(-DateTime.Today.Day + 1);
      _paymentsFinishDate = DateTime.Today.AddDays(1).AddSeconds(-1);
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
      DisplayName = "Keeper (c) 2012-13";
      Message = DateTime.Today.ToString("dddd , dd MMMM yyyy");
      OpenedAccountPage = 0;

      if (DbLoadResult.Code != 0)
      {
        MessageBox.Show("");
        MessageBox.Show(DbLoadResult.Explanation, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    public override void CanClose(Action<bool> callback)
    {
      if (DepositsFormPointer != null && DepositsFormPointer.IsActive) DepositsFormPointer.TryClose();
      StatusBarItem0 = DbSave.SaveAllTables().ToString();
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
      Db.AccountsPlaneList.Clear();
      Db.AccountsPlaneList = DbLoad.FillInAccountsPlaneList(Db.Accounts);
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

    public void ShowDeposit()
    {
      if (SelectedAccount.IsDescendantOf("Депозиты") && SelectedAccount.Children.Count == 0)
        WindowManager.ShowWindow(new DepositViewModel(SelectedAccount));
    }

    #endregion

    #region // вызовы дочерних окон

    public void ShowTransactionsForm()
    {
      String arcMessage = Message;
      Message = "Input operations";
      UsefulLists.FillLists();
      WindowManager.ShowDialog(new TransactionViewModel());
      // по возвращении на главную форму пересчитать остаток/оборот по выделенному счету/категории
      Period period = _openedAccountPage == 0 ? new Period(new DateTime(0), BalanceDate) : new Period(PaymentsStartDate, PaymentsFinishDate);
      Balance.CountBalances(SelectedAccount, period, BalanceList);
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

    public void ShowMonthAnalisysForm()
    {
      var arcMessage = Message;
      Message = "MonthAnalisys";
      WindowManager.ShowDialog(new MonthAnalisysViewModel());
      Message = arcMessage;
    }

    public void ShowDepositsForm()
    {
      if (DepositsFormPointer != null && DepositsFormPointer.IsActive) DepositsFormPointer.TryClose();
      DepositsFormPointer = new DepositsViewModel();
      WindowManager.ShowWindow(DepositsFormPointer);
    }
    #endregion

    #region // методы выгрузки / загрузки БД в текстовый файл
    public void SaveDatabase()
    {
      StatusBarItem0 = DbSave.SaveAllTables().ToString();
    }

    public void LoadDatabase()
    {
      TimeSpan elapsed;
      var result = DbLoad.LoadAllTables(out elapsed);
      StatusBarItem0 = elapsed.ToString();
      if (result.Code != 0) MessageBox.Show(result.Explanation, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

      InitVariablesToShowAccounts();
    }

    public void ClearDatabase()
    {
      DbClear.ClearAllTables();

      IncomesRoot.Clear();
      ExpensesRoot.Clear();
      ExternalAccountsRoot.Clear();
      MineAccountsRoot.Clear();
    }
    #endregion

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
        Balance.CountBalances(SelectedAccount, period, BalanceList);
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
        Balance.CountBalances(SelectedAccount, period, BalanceList);
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
        Balance.CountBalances(SelectedAccount, period, BalanceList);
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


    public void TempItem()
    {
      DbBinarySerialization();
      DbBinaryDeserialization();

      //      DbSoapSerialization();
      //      DbSoapDeserialization();

      //      DbXmlSerialization();
      //      DbXmlDeserialization();

      DbCryptoSerialization();
      DbCryptoDeserialization();
    }


    #region SOAP

    private static void DbSoapSerialization()
    {
      var watch1 = new Stopwatch();
      watch1.Start();

      var soapFormatter = new SoapFormatter();
      using (Stream fStream = new FileStream("KeeperDb.soap", FileMode.Create, FileAccess.Write))
      {
        soapFormatter.Serialize(fStream, Db);
      }

      watch1.Stop();
      Console.WriteLine("SoapFormatter serialization time is {0}", watch1.Elapsed);
    }

    private static void DbSoapDeserialization()
    {
      var watch1 = new Stopwatch();
      watch1.Start();

      var db2 = new KeeperTxtDb();
      var soapFormatter = new SoapFormatter();
      using (Stream fStream = new FileStream("KeeperDb.soap", FileMode.Open, FileAccess.Read))
      {
        db2 = (KeeperTxtDb)soapFormatter.Deserialize(fStream);
      }

      watch1.Stop();
      Console.WriteLine("SoapFormatter deserialization time is {0}", watch1.Elapsed);
    }

    #endregion

    #region XML serialization

    private static void DbXmlSerialization()
    {
      var watch1 = new Stopwatch();
      watch1.Start();

      var xmlSerializer = new XmlSerializer(typeof(KeeperTxtDb));
      using (Stream fStream = new FileStream("KeeperDb.xml", FileMode.Create, FileAccess.Write))
      {
        xmlSerializer.Serialize(fStream, Db);
      }

      watch1.Stop();
      Console.WriteLine("XmlSerializer serialization time is {0}", watch1.Elapsed);
    }

    private static void DbXmlDeserialization()
    {
      var watch1 = new Stopwatch();
      watch1.Start();

      var db1 = new KeeperTxtDb();
      var xmlSerializer = new XmlSerializer(typeof(KeeperTxtDb));
      using (Stream fStream = new FileStream("DbKeeper.xml", FileMode.Open, FileAccess.Read))
      {
        db1 = (KeeperTxtDb)xmlSerializer.Deserialize(fStream);
      }

      watch1.Stop();
      Console.WriteLine("XmlSerializer deserialization time is {0}", watch1.Elapsed);
    }

    #endregion

    #region Binary serialization

    private static void DbBinarySerialization()
    {
      var watch1 = new Stopwatch();
      watch1.Start();

      var binaryFormatter = new BinaryFormatter();
      using (Stream fStream = new FileStream("KeeperDb.binary", FileMode.Create, FileAccess.Write))
      {
        binaryFormatter.Serialize(fStream, Db);
      }

      watch1.Stop();
      Console.WriteLine("BinaryFormatter serialization  time is {0}", watch1.Elapsed);
    }

    private static void DbBinaryDeserialization()
    {
      var watch1 = new Stopwatch();
      watch1.Start();

      using (Stream fStream = new FileStream("KeeperDb.binary", FileMode.Open, FileAccess.Read))
      {
        var binaryFormatter = new BinaryFormatter();
        var db1 = (KeeperTxtDb)binaryFormatter.Deserialize(fStream);
      }

      watch1.Stop();
      Console.WriteLine("BinaryFormatter deserialization time is {0}", watch1.Elapsed);
    }

    #endregion

    #region Crypto

    private static void DbCryptoSerialization()
    {
      var watch1 = new Stopwatch();
      watch1.Start();

      byte[] key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };
      byte[] initVector = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };
      using (Stream fStream = new FileStream("KeeperDb.crypto", FileMode.Create, FileAccess.Write))
      {
        var rmCrypto = new RijndaelManaged();

        using (var cryptoStream = new CryptoStream(fStream, rmCrypto.CreateEncryptor(key, initVector), CryptoStreamMode.Write))
        {
          var binaryFormatter = new BinaryFormatter();
          binaryFormatter.Serialize(cryptoStream, Db);
        }

      }

      watch1.Stop();
      Console.WriteLine("BinaryFormatter serialization with Crypto takes {0} sec", watch1.Elapsed);
    }

    private static void DbCryptoDeserialization()
    {
      var watch1 = new Stopwatch();
      watch1.Start();

      byte[] key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };
      byte[] initVector = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };

      using (Stream fStream = new FileStream("KeeperDb.crypto", FileMode.Open, FileAccess.Read))
      {
        var rmCrypto = new RijndaelManaged();

        using (var cryptoStream = new CryptoStream(fStream,rmCrypto.CreateDecryptor(key,initVector),CryptoStreamMode.Read))
        {
          var binaryFormatter = new BinaryFormatter();
          var db1 = (KeeperTxtDb) binaryFormatter.Deserialize(cryptoStream);
        }
      }

      watch1.Stop();
      Console.WriteLine("BinaryFormatter deserialization with Crypto takes {0} sec", watch1.Elapsed);
    }

    #endregion
  }
}
