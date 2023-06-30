using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        TreeViewItemModel _draggedItem, _target;
        public AccountTreeView()
        {
            InitializeComponent();
        }

        private void treeView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            _draggedItem = (TreeViewItemModel)MyTreeView.SelectedItem;
            if (_draggedItem == null) return;
            DragDropEffects finalDropEffect = DragDrop.DoDragDrop(MyTreeView, MyTreeView.SelectedValue,
                DragDropEffects.Move);

            //Checking target is not null and item is dragging(moving)
            if ((finalDropEffect == DragDropEffects.Move) && (_target != null))
            {
                // A Move drop was accepted
                if (!_draggedItem.Name.Equals(_target.Name) && Keyboard.IsKeyDown(Key.LeftCtrl))
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
            e.Effects = CheckDropTarget(_draggedItem, ((TreeViewItemModel)item.Header)) 
                ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;
        }

        private void treeView_Drop(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;

            // Verify that this is a valid drop and then store the drop target
            TreeViewItem targetItem = GetNearestContainer(e.OriginalSource as UIElement);
            if (targetItem == null || _draggedItem == null) return;
            _target = ((TreeViewItemModel)targetItem.Header);
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

        private bool CheckDropTarget(TreeViewItemModel sourceItem, TreeViewItemModel targetItem)
        {
            if ((sourceItem).Parent == null)
                return false; // Root could not be moved!

            //Check whether the target item is meeting your condition
            return !sourceItem.Name.Equals(targetItem.Name);
        }

        //------------------------------------------------------------------------------

        private void DoAction(TreeViewItemModel source, TreeViewItemModel destination)
        {
            var vm = ((AccountTreeViewModel)DataContext).AskDragAccountActionViewModel;
            vm.Init(source.Name, destination.Name);
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

        private void MoveAccount(TreeViewItemModel source, TreeViewItemModel destination, Place place)
        {
            var sourceParent = (source).Parent;
            var destinationParent = (destination).Parent;

            sourceParent.Children.Remove(source);
            (source).Parent = destinationParent;
            PlaceIntoDestinationFolder(source, destination, place);
        }

        private void PlaceIntoDestinationFolder(TreeViewItemModel source, TreeViewItemModel destination, Place place)
        {
            var destinationParent = (destination).Parent;

            var tempAccount = new AccountItemModel(-1, "temporary", null);
            for (int i = destinationParent.Children.Count - 1; i >= 0; i--)
            {
                var item = destinationParent.Children[i];
                destinationParent.Children.RemoveAt(i);

                if ((item).Id == destination.Id && place == Place.After)
                    tempAccount.Children.Add(source);

                tempAccount.Children.Add(item);

                if (item.Id == (destination).Id && place == Place.Before)
                    tempAccount.Children.Add(source);
            }

            Comeback(tempAccount, destinationParent);
        }

        private static void Comeback(TreeViewItemModel tempAccountModel, TreeViewItemModel sourceParent)
        {
            for (int i = tempAccountModel.Children.Count - 1; i >= 0; i--)
            {
                var item = tempAccountModel.Children[i];
                tempAccountModel.Children.RemoveAt(i);
                sourceParent.Children.Add(item);
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            ((AccountTreeViewModel)DataContext).ShellPartsBinder.SelectedAccountModel =
                ((AccountTreeViewModel)DataContext).KeeperDataModel.GetSelectedAccountModel();
        }

        private void MoveIntoFolder(TreeViewItemModel source, TreeViewItemModel destination)
        {
            (source).Parent.Children.Remove(source);
            destination.Children.Add(source);
            (source).Parent = destination;
        }

    }
}
