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
  public class Balance
  {
    public string Field1 { get; set; }
    public string Field2 { get; set; }
    public string Field3 { get; set; }
  }

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
    private List<string> _stringList;

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

    public Account SelectedAccount
    {
      get { return _selectedAccount; }
      set
      {
        _selectedAccount = value;
        CountBalances();
      }
    }

    private void CountBalances()
    {
      BalancesList.Clear();
      var currentBalance = new Balance();
      currentBalance.Field1 = SelectedAccount.Name;
      BalancesList.Add(currentBalance);

      StringList.Clear();
      StringList.Add(SelectedAccount.Name);

      /* без учета валюты 
            var credit = Db.Transactions.Local.Where(t => t.Credit == SelectedAccount).Sum(t => t.Amount);
            var debet = Db.Transactions.Local.Where(t => t.Debet == SelectedAccount).Sum(t=>t.Amount);
            var balance = credit - debet;
            StringList.Add(balance.ToString());
      */
      var creditByCurrency = from t in Db.Transactions.Local
                              where t.Credit == SelectedAccount
                              group t by t.Currency into grouping
                              let count = grouping.Count()
                              select new
                                       {
                                         Currency = grouping.Key,
                                         Count = count
                                       };
      foreach (var item in creditByCurrency)
      {
        StringList.Add(String.Format("валюта {0} количество {1}", item.Currency,item.Count));
      }

      NotifyOfPropertyChange(() => StringList);
    }

    public ObservableCollection<Balance> BalancesList { get; set; }
    public List<string> StringList
    {
      get { return _stringList; }
      set
      {
        if (Equals(value, _stringList)) return;
        _stringList = value;
        NotifyOfPropertyChange(() => StringList);
      }
    }

    #endregion

    public ShellViewModel()
    {
      _message = "Keeper is running (On Debug)";
      Database.SetInitializer(new DbInitializer());
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

      BalancesList = new ObservableCollection<Balance>();
      var firstBalance = new Balance {Field1 = "bla-bla"};
      BalancesList.Add(firstBalance);

      StringList = new List<string> {"test string"};
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

    public void TemporaryLoadTransactionsView()
    {
      ShowTransactionsForm();
    }

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

  }
}
