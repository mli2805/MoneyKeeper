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
    public IWindowManager MyWindowManager { get; set; }

    [Import]
    public KeeperDb Db { get; set; }

    #region // поля/свойства в классе Модели к которым биндятся визуальные элементы из Вью

    // чисто по приколу, label на вьюхе, которая по ходу программы может меняться - поэтому свойство с нотификацией
    private string _message;

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
        ClearDb.RemoveAccountFromDatabase(SelectedAccount);
        if (SelectedAccount.Parent == null)
          AccountsRoots.Remove(SelectedAccount);
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
              MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
          ClearDb.RemoveCategoryFromDatabase(SelectedCategory);
      }
      else MessageBox.Show("Корневую категорию нельзя удалять!", "Отказ!");
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
      Category.CopyForEdit(categoryInWork, SelectedCategory);
      if (MyWindowManager.ShowDialog(new AddAndEditCategoryViewModel(categoryInWork, "Редактировать")) != true) return;

      if (SelectedCategory.Parent != categoryInWork.Parent)
      {
        categoryInWork.Parent.Children.Add(SelectedCategory);
        SelectedCategory.Parent.Children.Remove(SelectedCategory);
      }
      Category.CopyForEdit(SelectedCategory, categoryInWork);
    }

    #endregion

    #region // вызовы дочерних окон

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
      MyWindowManager.ShowDialog(new RatesViewModel());
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
      RestoreDb.RestoreAllTables();
      Db.SaveChanges();
      // и зачитать их для визуального отображения
      AccountsRoots = new ObservableCollection<Account>(from account in Db.Accounts.Local
                                                        where account.Parent == null
                                                        select account);
      IncomesRoots = new ObservableCollection<Category>(from category in Db.Categories.Local
                                                        //                                   where category.Parent == null 
                                                        where category.Name == "Все доходы"
                                                        select category);
      ExpensesRoots = new ObservableCollection<Category>(from category in Db.Categories.Local
                                                         where category.Name == "Все расходы"
                                                         select category);

      NotifyOfPropertyChange(() => AccountsRoots);
      NotifyOfPropertyChange(() => IncomesRoots);
      NotifyOfPropertyChange(() => ExpensesRoots);
    }

    public void ClearDatabase()
    {
      ClearDb.ClearAllTables();
      Db.SaveChanges();

      IncomesRoots.Clear();
      ExpensesRoots.Clear();
      AccountsRoots.Clear();
    }


    #endregion

  }
}
