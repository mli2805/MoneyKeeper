using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Keeper.Behaviors.ListViewBehaviors
{
    class ScrollToBottomOnResizeBehavior : Behavior<ListView>
    {
        protected override void OnAttached() // это стандартное место
        {
            AssociatedObject.SizeChanged += AssociatedObjectSizeChanged;
        }

        void AssociatedObjectSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (AssociatedObject.Items.Count == 0) return;
            AssociatedObject.ScrollIntoView(AssociatedObject.Items[AssociatedObject.Items.Count - 1]);
        }

    }
}
