using System.Composition;
using Caliburn.Micro;
using Keeper.Controls;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Transactions;
using System.Collections.Generic;
using System.Windows;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class ExpenseTranViewModel : Screen, IOneTranView
    {
        public TestControlVm ModelForControl { get; set; }

        public List<ButtonViewModel> Buttons { get; } = new List<ButtonViewModel>
        {
            new ButtonViewModel("First", () => MessageBox.Show("blah")),
            new ButtonViewModel("Second", () => MessageBox.Show("bluh"))
        };

        [ImportingConstructor]
        public ExpenseTranViewModel(KeeperDb db, BalanceDuringTransactionHinter balanceDuringTransactionHinter)
        {
            ModelForControl = new TestControlVm();
        }


        public void SetTran(TranWithTags tran)
        {
            ModelForControl.TextProperty = "Expense";
        }

        public void ButtonClose()
        {
            TryClose();
        }
    }
    public class ButtonViewModel
    {
        private readonly System.Action _action;
        public string Name { get; }

        public ButtonViewModel(string name, System.Action action)
        {
            _action = action;
            Name = name;
        }

        public void Click()
        {
            _action();
        }
    }
}