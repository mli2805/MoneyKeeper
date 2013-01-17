using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Keeper.Utils
{
  class ScrollToBottomOnLoadDatagridBehavior : Behavior<DataGrid>
  {
    protected override void OnAttached() 
    {
      AssociatedObject.Loaded += AssociatedObjectOnLoaded; 
    }

    private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
      AssociatedObject.ScrollIntoView(AssociatedObject.Items[AssociatedObject.Items.Count - 1]);
    }

  }
}
