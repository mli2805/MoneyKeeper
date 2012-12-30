using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
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
  public class ShellViewModel : Screen, IShell
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
        var period = new Period(new DateTime(0),BalanceDate);
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
      ObservableCollection<Account> page;
      switch (pageNumber)
      {
        case 0:
          page = MineAccountsRoot; break;
        case 1:
          page = ExternalAccountsRoot; break;
        case 2:
          page = IncomesRoot; break;
        case 3:
          page = ExpensesRoot; break;
        default:
          page = MineAccountsRoot; break;
      }

      var result = GetSelectedInCollection(page);

      if (result == null)
      {
        result = (from account in page
                  select account).FirstOrDefault();
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

    protected override void OnViewLoaded(object view)
    {
      DisplayName = "Keeper 2012";
      Message = DateTime.Today.ToString("dddd , dd MMMM yyyy");

      Db.Accounts.Load();  // загрузка с диска в оперативную
      Db.Transactions.Load();  // это должно происходить при загрузке главной формы
      Db.CurrencyRates.Load(); // пока эта форма главная

      InitVariablesToShowAccounts();
      OpenedAccountPage = 0;
      BalanceDate = DateTime.Today;
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

    #endregion

    #region // вызовы дочерних окон

    public void ShowTransactionsForm()
    {
      String arcMessage = Message;
      Message = "Input operations";
      WindowManager.ShowDialog(new TransactionViewModel());
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

    public DateTime BalanceDate 
    {
      get { return _balanceDate; }
      set
      {
        if (value.Equals(_balanceDate)) return;
        _balanceDate = value;
        NotifyOfPropertyChange(() => BalanceDate);
        var period = new Period(new DateTime(0), BalanceDate);
        Balance.CountBalances(SelectedAccount, period, BalanceList);
      }
    }

    public void TodayBalance() { BalanceDate = DateTime.Today; }
    public void YesterdayBalance() { BalanceDate = DateTime.Today.AddDays(-1); }
    public void LastMonthEndBalance() { BalanceDate = BalanceDate.AddDays(-BalanceDate.Day); }

    public void DecreaseOneDay() { BalanceDate = BalanceDate.AddDays(-1); }
    public void DecreaseOneMonth() { BalanceDate = BalanceDate.AddMonths(-1); }
    public void DecreaseOneYear() { BalanceDate = BalanceDate.AddYears(-1); }

    public void IncreaseOneDay() { BalanceDate = BalanceDate.AddDays(1); }
    public void IncreaseOneMonth() { BalanceDate = BalanceDate.AddMonths(1); }
    public void IncreaseOneYear() { BalanceDate = BalanceDate.AddYears(1); }
  }
}
