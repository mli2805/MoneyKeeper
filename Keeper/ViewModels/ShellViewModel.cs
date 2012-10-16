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
    public ObservableCollection<IncomeCategory> IncomesRoots { get; set; }
    public ObservableCollection<ExpenseCategory> ExpensesRoots { get; set; }

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
      IncomesRoots = new ObservableCollection<IncomeCategory>(from incomeCategory in Db.Incomes.Include("Children")
                                                              where incomeCategory.Parent == null
                                                              select incomeCategory);
      ExpensesRoots = new ObservableCollection<ExpenseCategory>(from expenseCategory in Db.Expenses.Include("Children")
                                                                where expenseCategory.Parent == null
                                                                select expenseCategory);

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
      if (MessageBox.Show("Are you sure?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) ==
          MessageBoxResult.Yes)
        SelectedCategory.Parent.Children.Remove(SelectedCategory);
    }

    public void AddCategory()
    {
      MyWindowManager.ShowDialog(new CategoryViewModel(SelectedCategory, FormMode.Create));
    }

    public void ChangeCategory()
    {
      MyWindowManager.ShowDialog(new CategoryViewModel(SelectedCategory, FormMode.Edit));
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

    public void DumpAccount(Account account, List<string> content)
    {
      foreach (var child in account.Children)
      {
        DumpAccount(child, content);
      }
      content.Add(account.ToDump());
    }

    public void ExportToTxt()
    {
      if (!Directory.Exists(Settings.Default.DumpPath)) Directory.CreateDirectory(Settings.Default.DumpPath);
      DumpAllAccounts();
      MessageBox.Show("Выгрузка завершена успешно!", "Экспорт");
    }

    public void DumpAllAccounts()
    {
      var content = new List<string>();
      foreach (var accountsRoot in AccountsRoots)
      {
        DumpAccount(accountsRoot, content);
      }
      File.WriteAllLines(Path.Combine(Settings.Default.DumpPath,"Accounts.txt"), content);
    }

    public void ImportFromTxt()
    {
      RestoreAllAccounts();
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

    public void RestoreAllAccounts()
    {
      string[] content = File.ReadAllLines(Path.Combine(Settings.Default.DumpPath,"Accounts.txt"));
      foreach (var s in content)
      {
        Console.WriteLine(s);
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
      foreach (var accountsRoot in AccountsRoots)
      {
        RemoveAccountFromDatabase(accountsRoot);
      }
      AccountsRoots.Clear();
    }

    #endregion

  }
}
