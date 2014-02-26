using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Keeper.Behaviors
{
  class ScrollToBottomOnLoadDatagridBehavior : Behavior<DataGrid>
  {
    protected override void OnAttached() 
    {
      AssociatedObject.Loaded += AssociatedObjectOnLoaded; 
    }

    private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
      if (AssociatedObject.Items.Count > 0)
      AssociatedObject.ScrollIntoView(AssociatedObject.Items[AssociatedObject.Items.Count - 1]);
    }
  }
}
