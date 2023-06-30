using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Keeper2018
{
    /// <summary>
    /// Interaction logic for AccountTreeView.xaml
    /// </summary>
    public partial class AccountTreeView
    {
        TreeViewItem _draggedItem, _target;
        public AccountTreeView()
        {
            InitializeComponent();
        }

        private void treeView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            _draggedItem = (TreeViewItem)MyTreeView.SelectedItem;
            if (_draggedItem == null) return;
            DragDropEffects finalDropEffect = DragDrop.DoDragDrop(MyTreeView, MyTreeView.SelectedValue,
                DragDropEffects.Move);

            //Checking target is not null and item is dragging(moving)
            if ((finalDropEffect == DragDropEffects.Move) && (_target != null))
            {
                // A Move drop was accepted
                if (!_draggedItem.Uid.Equals(_target.Uid) && Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    DoAction(_draggedItem, _target);
                    // CopyItem(_draggedItem, _target);
                    _target = null;
                    _draggedItem = null;
                }
            }
        }

        private void treeView_DragOver(object sender, DragEventArgs e)
        {
            // Verify that this is a valid drop and then store the drop target
            TreeViewItem item = GetNearestContainer(e.OriginalSource as UIElement);
            e.Effects = CheckDropTarget(_draggedItem, item) ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;
        }

        private void treeView_Drop(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;

            // Verify that this is a valid drop and then store the drop target
            TreeViewItem targetItem = GetNearestContainer(e.OriginalSource as UIElement);
            if (targetItem == null || _draggedItem == null) return;
            _target = targetItem;
            e.Effects = DragDropEffects.Move;
        }

        private TreeViewItem GetNearestContainer(UIElement element)
        {
            // Walk up the element tree to the nearest tree view item.
            TreeViewItem container = element as TreeViewItem;
            while ((container == null) && (element != null))
            {
                element = VisualTreeHelper.GetParent(element) as UIElement;
                container = element as TreeViewItem;
            }
            return container;
        }

        private bool CheckDropTarget(TreeViewItem sourceItem, TreeViewItem targetItem)
        {
            if (((AccountModel)sourceItem).Owner == null)
                return false; // Root could not be moved!

            //Check whether the target item is meeting your condition
            return !sourceItem.Uid.Equals(targetItem.Uid);
        }

        //------------------------------------------------------------------------------

        private void DoAction(TreeViewItem source, TreeViewItem destination)
        {
            var vm = ((AccountTreeViewModel)DataContext).AskDragAccountActionViewModel;
            vm.Init(source.Header.ToString(), destination.Header.ToString());
            ((AccountTreeViewModel)DataContext).WindowManager.ShowDialog(vm);

            switch (vm.Answer)
            {
                case DragAndDropAction.Before:
                    MoveAccount(source, destination, Place.Before);
                    break;
                case DragAndDropAction.Inside:
                    MoveIntoFolder(source, destination);
                    break;
                case DragAndDropAction.After:
                    MoveAccount(source, destination, Place.After);
                    break;
                case DragAndDropAction.Cancel: return;
                default: return;
            }
        }

        private void MoveAccount(TreeViewItem source, TreeViewItem destination, Place place)
        {
            var sourceParent = ((AccountModel)source).Owner;
            var destinationParent = ((AccountModel)destination).Owner;

            sourceParent.Items.Remove(source);
            ((AccountModel)source).Owner = destinationParent;
            PlaceIntoDestinationFolder(source, destination, place);
        }

        private void PlaceIntoDestinationFolder(TreeViewItem source, TreeViewItem destination, Place place)
        {
            var destinationParent = ((AccountModel)destination).Owner;

            var tempAccount = new AccountModel("temporary");
            for (int i = destinationParent.Items.Count - 1; i >= 0; i--)
            {
                var item = destinationParent.Items[i];
                destinationParent.Items.RemoveAt(i);

                if (((AccountModel)item).Id == ((AccountModel)destination).Id && place == Place.After)
                    tempAccount.Items.Add(source);

                tempAccount.Items.Add(item);

                if (((AccountModel)item).Id == ((AccountModel)destination).Id && place == Place.Before)
                    tempAccount.Items.Add(source);
            }

            Comeback(tempAccount, destinationParent);
        }

        private static void Comeback(AccountModel tempAccountModel, AccountModel sourceParent)
        {
            for (int i = tempAccountModel.Items.Count - 1; i >= 0; i--)
            {
                var item = tempAccountModel.Items[i];
                tempAccountModel.Items.RemoveAt(i);
                sourceParent.Items.Add(item);
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            ((AccountTreeViewModel)DataContext).ShellPartsBinder.SelectedAccountModel =
                ((AccountTreeViewModel)DataContext).KeeperDataModel.GetSelectedAccountModel();
        }

        private void MoveIntoFolder(TreeViewItem source, TreeViewItem destination)
        {
            ((AccountModel)source).Owner.Items.Remove(source);
            destination.Items.Add(source);
            ((AccountModel)source).Owner = (AccountModel)destination;
        }

    }
}
