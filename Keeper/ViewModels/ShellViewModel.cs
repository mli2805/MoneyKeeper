using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
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

    public ShellViewModel()
    {
      _message = "Keeper is running (On Debug)";
//      Database.SetInitializer(new DbInitializer());
      BalanceList = new ObservableCollection<string> { "test balance" };
    }

    public void OnImportsSatisfied()
    {
      StatusBarItem0 = DbLoad.LoadAllTables().ToString();

      InitVariablesToShowAccounts();
      _balanceDate = DateTime.Today.AddDays(1).AddSeconds(-1);
      _paymentsStartDate = DateTime.Today.AddDays(-DateTime.Today.Day + 1);
      _paymentsFinishDate = DateTime.Today.AddDays(1).AddSeconds(-1);
    }

    private void InitVariablesToShowAccounts()
    {
      // из копии в оперативке загружаем в переменные типа  ObservableCollection<Account>
      // при этом никакой загрузки не происходит - коллекция получает указатель на корневой Account
      // (могло быть несколько указателей на несколько корней дерева)
      // который при этом продолжает лежать в Db.Accounts и ссылаться на своих потомков
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
    }

    public override void CanClose(Action<bool> callback)
    {
      StatusBarItem0 = DbSave.SaveAllTables().ToString();
      callback(true);
    }

    #region // методы реализации контекстного меню на дереве счетов

    public void RemoveAccount()
    {
      if (SelectedAccount.Parent != null)
      {
        // такой запрос возвращает не коллекцию, а энумератор
        IEnumerable<Transaction> tr = from transaction in Db.Transactions
                                      where transaction.Debet == SelectedAccount || transaction.Credit == SelectedAccount || transaction.Article == SelectedAccount
                                      select transaction;

        // Any() пытается двинуться по этому энумератору и если может, то true
        if (tr.Any()) MessageBox.Show("Этот счет используется в проводках!", "Отказ!");
        else
        {
          if (MessageBox.Show("Удаление счета <<" + SelectedAccount.Name + ">>\n\n          Вы уверены?", "Confirm",
                              MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            DbClear.RemoveAccountFromDatabase(SelectedAccount);
        }

      }
      else MessageBox.Show("Корневой счет нельзя удалять!", "Отказ!");
    }

    public void AddAccount()
    {
      var accountInWork = new Account();
      accountInWork.Parent = SelectedAccount;
      if (WindowManager.ShowDialog(new AddAndEditAccountViewModel(accountInWork, "Добавить")) != true) return;

      SelectedAccount = accountInWork.Parent;
      SelectedAccount.Children.Add(accountInWork);
      Db.Accounts.Add(accountInWork);
    }

    public void ChangeAccount()
    {
      var accountInWork = new Account();
      Account.CopyForEdit(accountInWork, SelectedAccount);
      if (WindowManager.ShowDialog(new AddAndEditAccountViewModel(accountInWork, "Редактировать")) != true) return;

      if (SelectedAccount.Parent != accountInWork.Parent)
      {
        accountInWork.Parent.Children.Add(SelectedAccount);
        SelectedAccount.Parent.Children.Remove(SelectedAccount);
      }
      Account.CopyForEdit(SelectedAccount, accountInWork);
    }

    public void ShowDeposit()
    {
      if (SelectedAccount.IsDescendantOf("Депозиты") && SelectedAccount.Children.Count == 0)
                                WindowManager.ShowDialog(new DepositViewModel(SelectedAccount));
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
      String arcMessage = Message;
      Message = "Articles' associations";
      UsefulLists.FillLists();
      WindowManager.ShowDialog(new ArticlesAssociationsViewModel());
      Message = arcMessage;
    }

    public void ShowToDoForm()
    {
      String arcMessage = Message;
      Message = "TODO List";
      WindowManager.ShowDialog(new ToDoViewModel());
      Message = arcMessage;
    }

    public void ShowMonthAnalisysForm()
    {
      String arcMessage = Message;
      Message = "MonthAnalisys";
      WindowManager.ShowDialog(new MonthAnalisysViewModel());
      Message = arcMessage;
    }

    public void ShowDepositsForm()
    {
      String arcMessage = Message;
      Message = "All Deposits";
      WindowManager.ShowDialog(new DepositsViewModel());
      Message = arcMessage;
    }

    #endregion

    #region // методы выгрузки / загрузки БД в текстовый файл
    public void SaveDatabase()
    {
      StatusBarItem0 = DbSave.SaveAllTables().ToString();
    }

    public void LoadDatabase()
    {
      StatusBarItem0 = DbLoad.LoadAllTables().ToString();
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


    public void Load2002()
    {
      DbLoad.StartingBalances();
      DbLoad.Load2002D();
      DbLoad.Make2002Normal();
      DbLoad.Load2002Rk();
    }

  }
}
