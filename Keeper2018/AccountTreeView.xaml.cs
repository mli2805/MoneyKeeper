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
            _draggedItem = (TreeViewItem)AccountTree.SelectedItem;
            if (_draggedItem == null) return;
            DragDropEffects finalDropEffect = DragDrop.DoDragDrop(AccountTree, AccountTree.SelectedValue,
                DragDropEffects.Move);

            //Checking target is not null and item is dragging(moving)
            if ((finalDropEffect == DragDropEffects.Move) && (_target != null))
            {
                // A Move drop was accepted
                if (!_draggedItem.Header.ToString().Equals(_target.Header.ToString()))
                {
                    DoAction(_draggedItem, _target);
                    CopyItem(_draggedItem, _target);
                    _target = null;
                    _draggedItem = null;
                }
            }
        }

        private void DoAction(TreeViewItem source, TreeViewItem destination)
        {
            var vm = ((AccountTreeViewModel)DataContext).AskDragAccountActionViewModel;
            vm.Init(source.Header.ToString(), destination.Header.ToString());
            ((AccountTreeViewModel)DataContext).WindowManager.ShowDialog(vm);

            switch (vm.Answer)
            {
                case DragAndDropAction.Before: return;
                case DragAndDropAction.Inside: return;
                case DragAndDropAction.After: return;
                case DragAndDropAction.Cancel: return;
                default: return;
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
            //Check whether the target item is meeting your condition
            bool isEqual = !sourceItem.Header.ToString().Equals(targetItem.Header.ToString());
            return isEqual;

        }
        private void CopyItem(TreeViewItem sourceItem, TreeViewItem targetItem)
        {
            //Asking user whether he want to drop the dragged TreeViewItem here or not
            if (MessageBox.Show("Would you like to drop " + sourceItem.Header + " into " + targetItem.Header + "", "",
                    MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            //adding dragged TreeViewItem in target TreeViewItem
            AddChild(sourceItem, targetItem);

            //finding Parent TreeViewItem of dragged TreeViewItem 
            TreeViewItem parentItem = FindVisualParent(sourceItem);
            // if parent is null then remove from TreeView else remove from Parent TreeViewItem
            if (parentItem == null)
                AccountTree.Items.Remove(sourceItem);
            else
                parentItem.Items.Remove(sourceItem);
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

        //        static TObject FindVisualParent<TObject>(UIElement child) where TObject : UIElement
        //        {
        //            if (child == null)
        //                return null;
        //
        //            UIElement parent = VisualTreeHelper.GetParent(child) as UIElement;
        //            while (parent != null)
        //            {
        //                if (parent is TObject found)
        //                    return found;
        //                parent = VisualTreeHelper.GetParent(parent) as UIElement;
        //            }
        //            return null;
        //        }

        static TreeViewItem FindVisualParent(UIElement child)
        {
            if (child == null)
                return null;

            UIElement parent = VisualTreeHelper.GetParent(child) as UIElement;
            while (parent != null)
            {
                if (parent is TreeViewItem found)
                    return found;
                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
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
