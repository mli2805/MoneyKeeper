using System;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Interactivity;


namespace Keeper.Utils
{
  class ScrollToDisappearedDueDateChange : Behavior<ListView>
  {
    private int i = -1;
    protected override void OnAttached()
    {
      INotifyCollectionChanged itemCollection = AssociatedObject.Items;
      itemCollection.CollectionChanged += ItemCollectionOnCollectionChanged;
    }

    private void ItemCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
      Console.WriteLine(args.Action);
      
      if (i != -1 && i != AssociatedObject.SelectedIndex && args.Action == NotifyCollectionChangedAction.Reset)
      {
        if (AssociatedObject.SelectedIndex == -1) // в случае фильтрации, когда селектнутая строка не отобралась
          AssociatedObject.SelectedIndex = AssociatedObject.Items.Count - 1;
        AssociatedObject.ScrollIntoView(AssociatedObject.Items[AssociatedObject.SelectedIndex]);
      }

      i = AssociatedObject.SelectedIndex;
    }

  }
}
