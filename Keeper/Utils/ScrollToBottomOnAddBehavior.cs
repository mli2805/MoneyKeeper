using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Keeper.Utils
{
  class ScrollToBottomOnAddBehavior : Behavior<ListView>
  {
    protected override void OnAttached()
    {
      INotifyCollectionChanged itemCollection = AssociatedObject.Items;
      itemCollection.CollectionChanged += ItemCollectionOnCollectionChanged;
    }

    private void ItemCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
      var rows = (ItemCollection)sender;
      if (args.Action == NotifyCollectionChangedAction.Add)
        if (args.NewStartingIndex == rows.Count-1)
          AssociatedObject.ScrollIntoView(AssociatedObject.Items[AssociatedObject.Items.Count - 1]);
    }
  }
}
