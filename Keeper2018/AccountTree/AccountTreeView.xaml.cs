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
                if (!_draggedItem.Header.ToString().Equals(_target.Header.ToString()))
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

        private bool CheckDropTarget(TreeViewItem sourceItem, TreeViewItem targetItem)
        {
            if (((Account) sourceItem).Owner == null)
                return false; // Root could not be moved!

            //Check whether the target item is meeting your condition
            return !sourceItem.Header.ToString().Equals(targetItem.Header.ToString());
        }

        private void DoAction(TreeViewItem source, TreeViewItem destination)
        {
            var vm = ((AccountTreeViewModel)DataContext).AskDragAccountActionViewModel;
            vm.Init(source.Header.ToString(), destination.Header.ToString());
            ((AccountTreeViewModel)DataContext).WindowManager.ShowDialog(vm);

            switch (vm.Answer)
            {
                case DragAndDropAction.Before: 
                    PlaceBeforeAccount(source, destination);
                    return;
                case DragAndDropAction.Inside:
                    MoveIntoFolder(source, destination);
                    return;
                case DragAndDropAction.After: return;
                case DragAndDropAction.Cancel: return;
                default: return;
            }
        }

        private void PlaceBeforeAccount(TreeViewItem source, TreeViewItem destination)
        {
            var sourceParent = ((Account) source).Owner;
            var destinationParent = ((Account) destination).Owner;
            if (sourceParent.Id == destinationParent.Id)
                PlaceInSameFolder(source, destination);
            else F1();

        }

        private void F1(){}

        private void PlaceInSameFolder(TreeViewItem source, TreeViewItem destination)
        {
            var sourceParent = ((Account) source).Owner;

            var tempAccount = new Account(sourceParent.Header.ToString());

            var sourceAccount = ((Account) source);
            sourceParent.Items.Remove(sourceAccount);

            for (int i = sourceParent.Items.Count-1; i >= 0; i--)
            {
                
            }

            foreach (var child in sourceParent.Children)
            {
                if (child.Id == ((Account) destination).Id)
                {
                    tempAccount.Items.Add(sourceAccount);
                }

                sourceParent.Items.Remove(child);
                tempAccount.Items.Add(child);
            }

            for (int i = tempAccount.Items.Count-1; i >= 0; i--)
            {
                var item = tempAccount.Items[i];
                tempAccount.Items.RemoveAt(i);
                sourceParent.Items.Add(item);
            }
        }

        private void MoveIntoFolder(TreeViewItem source, TreeViewItem destination)
        {
            ((Account) source).Owner.Items.Remove(source);
            destination.Items.Add(source);
            ((Account) source).Owner = (Account)destination;
        }

        public void AddChild(TreeViewItem sourceItem, TreeViewItem targetItem)
        {
            // add item in target TreeViewItem 
            TreeViewItem item1 = new TreeViewItem();
            item1.Header = sourceItem.Header;
            targetItem.Items.Add(item1);
            foreach (TreeViewItem item in sourceItem.Items)
            {
                AddChild(item, item1);
            }
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
    }
}
