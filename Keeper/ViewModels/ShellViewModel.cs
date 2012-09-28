using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

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
      MyWindowManager.ShowDialog(new AddOperationViewModel());
      Message = arcMessage;
    }

    public void CloseProgram()
    {
      if (MessageBox.Show("Are you sure?","Confirm",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes )
        TryClose();
    }

    

  }
}
