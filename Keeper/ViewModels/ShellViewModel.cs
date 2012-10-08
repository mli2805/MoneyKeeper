using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  [Export(typeof(IShell))]
  [Export(typeof(ShellViewModel)),PartCreationPolicy(CreationPolicy.Shared)]
  public class ShellViewModel : Screen, IShell
  {
    [Import]
    public IWindowManager MyWindowManager { get; set; }

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
      AccountsRoots = new ObservableCollection<Account>();
      IncomesRoots = new ObservableCollection<Category>();
      ExpensesRoots = new ObservableCollection<Category>();
    }


    #region // подготовка списков счетов и категорий
    private void PrepareAccountsTree()
    {
      var account = new Account("Все");
      var account1 = new Account("Депозиты");
      account.Children.Add(new Account("Кошельки"));
      account.Children.Add(account1);
      account.IsAggregate = true;
      account1.Parent = account;
      var account2 = new Account("АСБ \"Ваш выбор\" 14.01.2012 -");
      account1.Children.Add(account2);
      account1.IsAggregate = true;
      account2.Parent = account1;
      AccountsRoots.Add(account);
      account.IsSelected = true;
    }

    private void PrepareIncomesCategoriesTree()
    {
      var category = new Category("Зарплата");
      IncomesRoots.Add(category);
      var category1 = new Category("Зарплата моя официальная");
      category.Children.Add(category1);
      category1 = new Category("Зарплата моя 2я часть");
      category.Children.Add(category1);
      category1 = new Category("Процентные доходы");
      IncomesRoots.Add(category1);
    }

    private void PrepareExpensesCategoriesTree()
    {
      var category = new Category("Все расходы");
      ExpensesRoots.Add(category);
      var category1 = new Category("Продукты");
      category.Children.Add(category1);
      category1 = new Category("Автомобиль");
      category.Children.Add(category1);
      category1 = new Category("Коммунальные платежи");
      category.Children.Add(category1);

    }
    #endregion

    protected override void OnViewLoaded(object view)
    {
      DisplayName = "Keeper 2012";
      PrepareAccountsTree();
      PrepareIncomesCategoriesTree();
      PrepareExpensesCategoriesTree();
    }

    #region // методы реализации контекстного меню на дереве счетов
    public void RemoveAccount()
    {
      if (MessageBox.Show("Are you sure?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        SelectedAccount.Parent.Children.Remove(SelectedAccount);
    }
    
    public void AddAccount()
    {
      MyWindowManager.ShowDialog(new AddAccountViewModel(SelectedAccount));
    }

    public void ChangeAccount()
    {
      MyWindowManager.ShowDialog(new AddAccountViewModel(SelectedAccount));
    }
    #endregion

    public void ShowTransactionsForm()
    {
      String arcMessage = Message;
      Message = "Input operations";
      MyWindowManager.ShowDialog(new TransactionsViewModel());
      Message = arcMessage;
    }

  }
}
