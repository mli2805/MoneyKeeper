using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Keeper.Behaviors.DataGridBehaviors
{
    class ScrollToBottomOnLoadDatagridBehavior : Behavior<DataGrid>
    {
        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObjectOnLoaded;
            Console.WriteLine("ScrollToBottomOnLoadDatagridBehavior attached");
        }

        private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (AssociatedObject.Items.Count > 0)
                AssociatedObject.ScrollIntoView(AssociatedObject.Items[AssociatedObject.Items.Count - 1]);

            Console.WriteLine("ScrollToBottomOnLoadDatagridBehavior works");
        }
    }
}
