using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Keeper2018
{
    public class ScrollListViewToBottomOnAddBehavior : Behavior<ListView>
    {
        protected override void OnAttached()
        {
            INotifyCollectionChanged itemCollection = AssociatedObject.Items;
            itemCollection.CollectionChanged += ItemCollectionOnCollectionChanged;
        }

        private void ItemCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Reset) return; // Reset приходит при начальной инициализации Rows и обрабатывается в ScrollToPreviousExitPointOrBottomOnLoadBehavior
            if (AssociatedObject.Items.Count > 0)
                AssociatedObject.ScrollIntoView(AssociatedObject.Items[AssociatedObject.Items.Count - 1]);
        }
    }
}