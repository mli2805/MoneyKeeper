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
  public class ShellViewModel : Screen, IShell
  {
    [Import]
    public IWindowManager MyWindowManager { get; set; }

    public ShellViewModel()
    {
      _message = "Keeper is running (On Debug)";

      PrepareAccountsTree();
    }

    // во ViewModel создается public property к которому будет биндиться компонент из View
    // далее содержимое этого свойства изменяется и это должно быть отображено на экране
    // поэтому вместо обычного свойства решарпером создаем свойство с нотификацией
    public ObservableCollection<Account> AccountsRoots { get; set; }

    private void PrepareAccountsTree()
    {
      AccountsRoots = new ObservableCollection<Account>();
      var account = new Account("Все");
      var account1 = new Account("Депозиты");
      account.Children.Add(new Account("Кошельки"));
      account.Children.Add(account1);
      account1.Parent = account;
      account1.Children.Add(new Account("АСБ \"Ваш выбор\" 14.01.2012 -"));
      AccountsRoots.Add(account);

//      IncomesRoots = new IObservableCollection<>
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = "Keeper 2012";
    }

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

    public void AddAccount()
    {
      MyWindowManager.ShowDialog(new AddAccountViewModel());
    }

    public void RunOperationsForm()
    {
      ShowTransactionsForm();
    }

    public void ShowTransactions()
    {
      ShowTransactionsForm();
    }

    private void ShowTransactionsForm()
    {
      String arcMessage = Message;
      Message = "Input operations";
      MyWindowManager.ShowDialog(new AddIncomeViewModel());
      Message = arcMessage;
    }

    public void CloseProgram()
    {
      if (MessageBox.Show("Are you sure?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        TryClose();
    }



  }
}
