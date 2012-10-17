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

namespace Keeper.ViewModels
{
  [Export(typeof (IShell))]
  [Export(typeof (ShellViewModel)), PartCreationPolicy(CreationPolicy.Shared)]
  public class ShellViewModel : Screen, IShell
  {
    [Import]
    public IWindowManager MyWindowManager { get; set; }

    [Import]
    public KeeperDb Db { get; set; }

    #region // поля/свойства в классе Модели к которым биндятся визуальные элементы из Вью

    // чисто по приколу, label на вьюхе, которая по ходу программы может меняться - поэтому свойство с нотификацией
    private string _message;

    public String Message
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
    public ObservableCollection<Account> AccountsRoots { get; set; }
    public ObservableCollection<Category> IncomesRoots { get; set; }
    public ObservableCollection<Category> ExpensesRoots { get; set; }

    public Account SelectedAccount { get; set; }
    public Category SelectedCategory { get; set; }

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

      AccountsRoots = new ObservableCollection<Account>(from account in Db.Accounts.Include("Children")
                                                        where account.Parent == null
                                                        select account);
      IncomesRoots = new ObservableCollection<Category>(from category in Db.Categories.Include("Children")
//                                                        where category.Parent == null 
                                                        where category.Name == "Все доходы"
                                                        select category);
      ExpensesRoots = new ObservableCollection<Category>(from category in Db.Categories.Include("Children")
                                                         where category.Name == "Все расходы"
                                                         select category);

      NotifyOfPropertyChange(() => AccountsRoots);
      NotifyOfPropertyChange(() => IncomesRoots);
      NotifyOfPropertyChange(() => ExpensesRoots);
    }

    #region // методы реализации контекстного меню на дереве счетов

    public void RemoveAccount()
    {
      if (
        MessageBox.Show("Удаление счета <<" + SelectedAccount.Name + ">>\n\n          Вы уверены?", "Confirm",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
      {
        if (SelectedAccount.Parent != null)
          SelectedAccount.Parent.Children.Remove(SelectedAccount);
        else
        {
          RemoveAccountFromDatabase(SelectedAccount);
          AccountsRoots.Remove(SelectedAccount);
        }
      }
    }

    public void AddAccount()
    {
      var accountInWork = new Account();
      accountInWork.Parent = SelectedAccount;
      if (MyWindowManager.ShowDialog(new AddAndEditAccountViewModel(accountInWork, "Добавить")) != true) return;

      SelectedAccount.Children.Add(accountInWork);
    }

    public void ChangeAccount()
    {
      var accountInWork = new Account();
      Account.CopyForEdit(accountInWork, SelectedAccount);
      if (MyWindowManager.ShowDialog(new AddAndEditAccountViewModel(accountInWork, "Редактировать")) != true) return;

      if (SelectedAccount.Parent != accountInWork.Parent)
      {
        //        обязательно в таком порядке - сначала добавить в новое место потом удалить из старого, 
        //        если сначала удалить у старого родителя, то инстанс пропадает из памяти и новому добавляется null
        if (accountInWork.Parent != null) accountInWork.Parent.Children.Add(SelectedAccount);
        else AccountsRoots.Add(SelectedAccount);
        if (SelectedAccount.Parent != null) SelectedAccount.Parent.Children.Remove(SelectedAccount);
        else AccountsRoots.Remove(SelectedAccount);
      }
      Account.CopyForEdit(SelectedAccount, accountInWork);
    }

    #endregion

    #region // методы реализации контекстного меню на дереве категорий

    public void RemoveCategory()
    {
      if (SelectedCategory.Parent != null)
      {
        if (MessageBox.Show("Удаление категории <<" + SelectedCategory.Name + ">>\n\n          Вы уверены?", "Confirm",
          MessageBoxButton.YesNo, MessageBoxImage.Question) ==
            MessageBoxResult.Yes)
          SelectedCategory.Parent.Children.Remove(SelectedCategory);
      }
      else MessageBox.Show("Корневую категорию нельзя удалять!","Отказ!");
    }

    public void AddCategory()
    {
      var categoryInWork = new Category();
      categoryInWork.Parent = SelectedCategory;
      if (MyWindowManager.ShowDialog(new AddAndEditCategoryViewModel(categoryInWork, "Добавить")) != true) return;

      SelectedCategory.Children.Add(categoryInWork);
      Db.Categories.Add(categoryInWork);
    }

    public void ChangeCategory()
    {
      var categoryInWork = new Category();
      Category.CopyForEdit(categoryInWork,SelectedCategory);  
      if (MyWindowManager.ShowDialog(new AddAndEditCategoryViewModel(categoryInWork, "Редактировать")) != true) return;

      if (SelectedCategory.Parent != categoryInWork.Parent)
      {
        categoryInWork.Parent.Children.Add(SelectedCategory);
        SelectedCategory.Parent.Children.Remove(SelectedCategory);
      }
      Category.CopyForEdit(SelectedCategory,categoryInWork);
    }

    #endregion

    public void ShowTransactionsForm()
    {
      String arcMessage = Message;
      Message = "Input operations";
      MyWindowManager.ShowDialog(new TransactionsViewModel());
      Message = arcMessage;
    }

    public void ShowCurrencyRatesForm()
    {
      String arcMessage = Message;
      Message = "Currency rates";
      MyWindowManager.ShowDialog(new TransactionsViewModel());
      Message = arcMessage;
    }

    #region // методы выгрузки / загрузки БД в текстовый файл

    #region  // выгрузка в текстовый файл, пока только Счета и Курсы
    public void ExportToTxt()
    {
      if (!Directory.Exists(Settings.Default.DumpPath)) Directory.CreateDirectory(Settings.Default.DumpPath);
      DumpAccounts();
      DumpCurrencyRates();
      MessageBox.Show("Выгрузка завершена успешно!", "Экспорт");
    }

    public void DumpAccounts()
    {
      var content = new List<string>();
      foreach (var accountsRoot in AccountsRoots)
      {
        DumpAccount(accountsRoot, content);
      }
      File.WriteAllLines(Path.Combine(Settings.Default.DumpPath, "Accounts.txt"), content);
    }

    public void DumpAccount(Account account, List<string> content)
    {
      foreach (var child in account.Children)
      {
        DumpAccount(child, content);
      }
      content.Add(account.ToDump());
    }

    public void DumpCurrencyRates()
    {
      var content = new List<string>();
      foreach (var currencyRate in Db.CurrencyRates)
      {
        content.Add(currencyRate.ToDump());
      }
      File.WriteAllLines(Path.Combine(Settings.Default.DumpPath, "CurrencyRates.txt"), content);
    }

    #endregion // выше идут выгрузки в текстовый файл

    #region //  загрузка таблицы счетов из текстового файла
    public void ImportFromTxt()
    {
      RestoreCurrencyRates();
      RestoreAccounts();
    }

    public void RestoreCurrencyRates()
    {
      string[] content = File.ReadAllLines(Path.Combine(Settings.Default.DumpPath, "CurrencyRates.txt"));
      foreach (var s in content)
      {
        var rate = CurrencyRateFromString(s);
        Db.CurrencyRates.Add(rate);
      }
    }

    private CurrencyRate CurrencyRateFromString(string s)
    {
      var rate = new CurrencyRate();
      int prev = s.IndexOf(',');
      rate.BankDay = Convert.ToDateTime(s.Substring(0, prev));
      prev = s.IndexOf(',', prev + 2);
      rate.Currency = (CurrencyCodes)Enum.Parse(typeof(CurrencyCodes), s.Substring(prev + 2, 3));
      prev += 6;
      int next = s.IndexOf(',', prev + 2);
      rate.Rate = Convert.ToDecimal(s.Substring(prev));
      return rate;
    }

    public void RestoreAccounts()
    {
      string[] content = File.ReadAllLines(Path.Combine(Settings.Default.DumpPath, "Accounts.txt"));
      foreach (var s in content)
      {
        int parentId;
        var account = AccoutFromString(s, out parentId);
        if (parentId == 0)
        {
          BuildBranchFromRoot(account, content);
          AccountsRoots.Add(account);
          Db.Accounts.Add(account);
        }
      }
    }

    private Account AccoutFromString(string s, out int parentId)
    {
      var account = new Account();
      int prev = s.IndexOf(',');
      account.Id = Convert.ToInt32(s.Substring(0, prev));
      int next = s.IndexOf(',', prev + 2);
      account.Name = s.Substring(prev + 2, next-prev-3);
      prev = next;
      account.Currency = (CurrencyCodes) Enum.Parse(typeof(CurrencyCodes), s.Substring(prev + 2, 3));
      prev += 6;
      next = s.IndexOf(',', prev + 2);
      parentId = Convert.ToInt32(s.Substring(prev + 2, next - prev - 3));
      prev = next;
      account.IsAggregate = Convert.ToBoolean(s.Substring(prev + 2));
      return account;
    }

    private void BuildBranchFromRoot(Account root, string[] content)
    {
      foreach (var s in content)
      {
        int parentId;
        var account = AccoutFromString(s, out parentId);
        if (parentId == root.Id)
        {
          account.Parent = root;
          root.Children.Add(account);
          BuildBranchFromRoot(account, content);
        }
      }
    }

    #endregion // загрузка

    #region //  очистка 3 таблиц в БД (кроме категорий доходов-расходов)
    public void RemoveAccountFromDatabase(Account account)
    {
      foreach (var child in account.Children.ToArray())
      {
        RemoveAccountFromDatabase(child);
      }
      Db.Accounts.Remove(account);
    }

    public void ClearDatabase()
    {
      ClearCurrencyRatesTable();
      ClearTransactionsTable();
      ClearAccountsTable();
    }

    private void ClearCurrencyRatesTable()
    {
      foreach (var currencyRate in Db.CurrencyRates.ToArray())
      {
        Db.CurrencyRates.Remove(currencyRate);
      }
    }

    private  void ClearTransactionsTable()
    {
      foreach (var transaction in Db.Transactions.ToArray())
      {
        Db.Transactions.Remove(transaction);
      }
    }

    private void ClearAccountsTable()
    {
      foreach (var accountsRoot in AccountsRoots)
      {
        RemoveAccountFromDatabase(accountsRoot);
      }
      AccountsRoots.Clear();
    }
    #endregion

    #endregion

  }
}
