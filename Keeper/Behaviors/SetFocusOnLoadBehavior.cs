using System.Windows;
using System.Windows.Interactivity;

namespace Keeper.Behaviors
{
  public class SetFocusOnLoadBehavior : Behavior<FrameworkElement>
  {
    protected override void OnAttached()
    {
      AssociatedObject.Loaded +=AssociatedObjectOnLoaded;
    }

    private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
      AssociatedObject.Focus();
    }
  }
}
