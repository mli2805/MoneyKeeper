﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Properties;
using Keeper.Utils;

namespace Keeper.ViewModels
{
  [Export(typeof(IShell))]
  [Export(typeof(ShellViewModel)), PartCreationPolicy(CreationPolicy.Shared)]
  public class ShellViewModel : Screen, IShell, IPartImportsSatisfiedNotification
  {
    [Import]
    public IWindowManager WindowManager { get; set; }

    [Import]
    public KeeperDb Db { get; set; }

    #region // поля/свойства в классе Модели к которым биндятся визуальные элементы из Вью

    // чисто по приколу, label на вьюхе, которая по ходу программы может меняться - поэтому свойство с нотификацией
    private string _message;
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
      Database.SetInitializer(new DbInitializer());
      BalanceList = new ObservableCollection<string>() { "test balance" };
    }

    public override void CanClose(Action<bool> callback)
    {
      Db.SaveChanges();
      Db.Dispose();
      callback(true);
    }

    public void OnImportsSatisfied()
    {
      Db.Accounts.Load();  // загрузка с диска в оперативную
      Db.Transactions.Load();  // это должно происходить при загрузке главной формы
      Db.CurrencyRates.Load(); // пока эта форма главная
      Db.ArticlesAssociations.Load();

      InitVariablesToShowAccounts();
      _balanceDate = DateTime.Today.AddDays(1).AddSeconds(-1);
      _paymentsStartDate = DateTime.Today.AddDays(-DateTime.Today.Day + 1);
      _paymentsFinishDate = DateTime.Today.AddDays(1).AddSeconds(-1);
      OpenedAccountPage = 0;
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = "Keeper 2012";
      Message = DateTime.Today.ToString("dddd , dd MMMM yyyy");
    }

    private void InitVariablesToShowAccounts()
    {
      // из копии в оперативке загружаем в переменные типа  ObservableCollection<Account>
      // при этом никакой загрузки не происходит - коллекция получает указатель на корневой Account
      // (могло быть несколько указателей на несколько корней дерева)
      // который при этом продолжает лежать в Db.Accounts.Local и ссылаться на своих потомков
      MineAccountsRoot = new ObservableCollection<Account>(from account in Db.Accounts.Local
                                                           where account.Name == "Мои"
                                                           select account);
      ExternalAccountsRoot = new ObservableCollection<Account>(from account in Db.Accounts.Local
                                                               where account.Name == "Внешние" || account.Name == "Для ввода стартовых остатков"
                                                               select account);
      IncomesRoot = new ObservableCollection<Account>(from account in Db.Accounts.Local
                                                      where account.Name == "Все доходы"
                                                      select account);
      ExpensesRoot = new ObservableCollection<Account>(from account in Db.Accounts.Local
                                                       where account.Name == "Все расходы"
                                                       select account);

      NotifyOfPropertyChange(() => MineAccountsRoot);
      NotifyOfPropertyChange(() => ExternalAccountsRoot);
      NotifyOfPropertyChange(() => IncomesRoot);
      NotifyOfPropertyChange(() => ExpensesRoot);
    }

    #region // методы реализации контекстного меню на дереве счетов

    public void RemoveAccount()
    {
      if (SelectedAccount.Parent != null)
      {
        // такой запрос возвращает не коллекцию, а энумератор
        IEnumerable<Transaction> tr = from transaction in Db.Transactions.Local
                                      where transaction.Debet == SelectedAccount || transaction.Credit == SelectedAccount
                                      select transaction;

        // Any() пытается двинуться по этому энумератору и если может, то true
        if (tr.Any()) MessageBox.Show("Этот счет используется в проводках!", "Отказ!");
        else
        {
          if (MessageBox.Show("Удаление счета <<" + SelectedAccount.Name + ">>\n\n          Вы уверены?", "Confirm",
                              MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            ClearDb.RemoveAccountFromDatabase(SelectedAccount);
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
      WindowManager.ShowDialog(new DepositViewModel(SelectedAccount));
    }

    #endregion

    #region // вызовы дочерних окон

    public void ShowTransactionsForm()
    {
      String arcMessage = Message;
      Message = "Input operations";
      WindowManager.ShowDialog(new TransactionViewModel());
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

    public void ArticlesAssociations()
    {
      String arcMessage = Message;
      Message = "Articles' associations";
      WindowManager.ShowDialog(new ArticlesAssociationsViewModel());
      Message = arcMessage;
    }
    #endregion

    #region // методы выгрузки / загрузки БД в текстовый файл
    public void DumpDatabaseToTxt()
    {
      Db.SaveChanges(); // сначала сохранить текущие изменения из ОЗУ на винт, при этом новые записи получат ID,
      DumpDb.DumpAllTables();  // затем уже выгрузить
    }

    public void RestoreDatabaseFromTxt()
    {
      // загружает из текстовых файлов данные в копии таблиц БД в оперативке (db.xxxxx.local)
      RestoreDb.RestoreAllTables();
      // записывает эти данные в БД на винт
      Db.SaveChanges();
      // инициализирует переменные для визуального отображения деревьев счетов
      InitVariablesToShowAccounts();
    }

    public void ClearDatabase()
    {
      ClearDb.ClearAllTables();
      Db.SaveChanges();

      IncomesRoot.Clear();
      ExpensesRoot.Clear();
      ExternalAccountsRoot.Clear();
      MineAccountsRoot.Clear();
    }
    #endregion


    #region // одноразовые методы
    public static Encoding Encoding1251 = Encoding.GetEncoding(1251);
    // сравнение ежедневных остатков с выгруженными из старого кипера
    public void DayBalances()
    {
      string[] content = File.ReadAllLines(Path.Combine(Settings.Default.DumpPath, "OstatkiDnevn.txt"), Encoding1251);
      var acc = new Account[15];
      var cur = new CurrencyCodes[15];
      acc[0] = (from account in Db.Accounts.Local where account.Name == "Мой кошелек" select account).First(); cur[0] = CurrencyCodes.BYR;
      acc[1] = (from account in Db.Accounts.Local where account.Name == "Юлин кошелек" select account).First(); cur[1] = CurrencyCodes.BYR;
      acc[2] = (from account in Db.Accounts.Local where account.Name == "Мой кошелек" select account).First(); cur[2] = CurrencyCodes.USD;
      acc[3] = (from account in Db.Accounts.Local where account.Name == "Шкаф" select account).First(); cur[3] = CurrencyCodes.BYR;
      acc[4] = (from account in Db.Accounts.Local where account.Name == "Шкаф" select account).First(); cur[4] = CurrencyCodes.USD;
      acc[5] = (from account in Db.Accounts.Local where account.Name == "Шкаф единички" select account).First(); cur[5] = CurrencyCodes.USD;
      acc[6] = (from account in Db.Accounts.Local where account.Name == "Шкаф" select account).First(); cur[6] = CurrencyCodes.EUR;
      acc[7] = (from account in Db.Accounts.Local where account.Name == "Мне должны" select account).First(); cur[7] = CurrencyCodes.BYR;
      acc[8] = (from account in Db.Accounts.Local where account.Name == "Мне должны" select account).First(); cur[8] = CurrencyCodes.USD;
      acc[9] = (from account in Db.Accounts.Local where account.Name == "Тумбочка" select account).First(); cur[9] = CurrencyCodes.BYR;
      acc[10] = (from account in Db.Accounts.Local where account.Name == "Моя карточка" select account).First(); cur[10] = CurrencyCodes.BYR;
      acc[11] = (from account in Db.Accounts.Local where account.Name == "Юлина карточка" select account).First(); cur[11] = CurrencyCodes.BYR;
      //      acc[12] = (from account in Db.Accounts.Local where account.Name == "Депозиты закрытые до ведения в данной программе" select account).First(); cur[12] = CurrencyCodes.USD;
      //      acc[13] = (from account in Db.Accounts.Local where account.Name == "Депозиты закрытые до ведения в данной программе" select account).First(); cur[13] = CurrencyCodes.BYR;
      //      acc[14] = (from account in Db.Accounts.Local where account.Name == "Депозиты закрытые до ведения в данной программе" select account).First(); cur[14] = CurrencyCodes.EUR;
      acc[12] = (from account in Db.Accounts.Local where account.Name == "Депозиты" select account).First(); cur[12] = CurrencyCodes.USD;
      acc[13] = (from account in Db.Accounts.Local where account.Name == "Депозиты" select account).First(); cur[13] = CurrencyCodes.BYR;
      acc[14] = (from account in Db.Accounts.Local where account.Name == "Депозиты" select account).First(); cur[14] = CurrencyCodes.EUR;

      foreach (var s in content)
      {
        var dt = new DateTime();
        var ost = new decimal[15];
        ParseOstatki(s, out dt, ost);
        decimal[] bal;
        GetBalances(dt.AddDays(1).AddSeconds(-1), acc, cur, out bal);
        int a = CompareOstatki(ost, bal);
        if (a != -1)
        {
          MessageBox.Show(String.Format("{0:dd/MMM/yyyy}  счет {1} в {2}", dt, acc[a], cur[a]));
          BalanceDate = dt;
          break;
        }
      }
    }

    public void ParseOstatki(string st, out DateTime dt, decimal[] ost)
    {
      int prev = 0;
      int next = st.IndexOf(';', prev);
      dt = Convert.ToDateTime(st.Substring(prev, next));

      for (int i = 0; i < 15; i++)
      {
        prev = next + 2;
        next = st.IndexOf(';', prev);
        ost[i] = Convert.ToDecimal(st.Substring(prev, next - prev - 1));
      }

    }

    public void GetBalances(DateTime dt, Account[] acc, CurrencyCodes[] cur, out decimal[] bal)
    {
      var start = new DateTime(2001, 01, 01);
      bal = new decimal[15];
      for (int i = 0; i < acc.Count(); i++)
        bal[i] = Balance.GetBalanceInCurrency(acc[i], new Period(start, dt), cur[i]);
    }

    public int CompareOstatki(decimal[] ost, decimal[] bal)
    {
      for (int i = 0; i < ost.Count(); i++)
        if (ost[i] != bal[i]) return i;
      return -1;
    }

    public void NegativeBalances()
    {
      var content = new List<string>();
      var orderedTransactions = from transaction in Db.Transactions
//                                where transaction.Debet.IsDescendantOf("Мои")
                                orderby transaction.Timestamp
                                select transaction;

      foreach (var transaction in orderedTransactions)
      {
        if (!transaction.Debet.IsDescendantOf("Мои")) continue;
        var balance = Balance.GetBalanceInCurrency(transaction.Debet,
                                                   new Period(new DateTime(0), transaction.Timestamp.AddSeconds(59)),
                                                   transaction.Currency);
        if (balance < 0)
          content.Add(String.Format("{0:dd/MM/yyyy hh:mm} со счета {1} списано {2} {3} осталось {4}",
            transaction.Timestamp, transaction.Debet, transaction.Amount, transaction.Currency, balance));
      }
      File.WriteAllLines(Path.Combine(Settings.Default.DumpPath, "NegativeBalances.txt"), content, Encoding1251);

    }

    public void ShamTransactions()
    {
      var content = new List<string>();

      foreach (var transaction in Db.Transactions) 
      {
        if (transaction.Comment.IndexOf("фикт", System.StringComparison.Ordinal) != -1) 
          content.Add(transaction.ToDumpWithNames());
      }
      File.WriteAllLines(Path.Combine(Settings.Default.DumpPath, "ShamTransactions.txt"), content, Encoding1251);
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

  }


}
