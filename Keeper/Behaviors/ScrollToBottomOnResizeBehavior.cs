using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Keeper.Behaviors
{
  class ScrollToBottomOnResizeBehavior : Behavior<ListView>
  {
    protected override void OnAttached() // это стандартное место
    {
//      AssociatedObject.Loaded += AssociatedObjectOnLoaded;  // а здесь подписываюсь именно на то событие, по которому должно выполняться действие
      AssociatedObject.SizeChanged += AssociatedObjectSizeChanged;
    }

    void AssociatedObjectSizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (AssociatedObject.Items.Count == 0) return;
      AssociatedObject.ScrollIntoView(AssociatedObject.Items[AssociatedObject.Items.Count - 1]);
    }

  }
}
