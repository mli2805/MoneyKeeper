using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
      
      if (i != AssociatedObject.SelectedIndex && args.Action == NotifyCollectionChangedAction.Reset)
      {
        AssociatedObject.ScrollIntoView(AssociatedObject.Items[AssociatedObject.SelectedIndex]);
      }

      i = AssociatedObject.SelectedIndex;
    }

  }
}
