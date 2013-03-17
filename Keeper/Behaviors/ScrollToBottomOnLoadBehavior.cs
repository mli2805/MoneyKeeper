using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Keeper.Behaviors
{
  /// <summary>
  /// Этот Behavior аттачится на ListView (т.е. AssociatedObject это ListView)
  /// 
  /// В момент аттаченья Behavior подписывается на метод OnLoaded объекта с котором он проассоциирован
  /// значит когда ListView загрузится (Loaded) вызовется мой код 
  /// в котором сказано проскролить ListView чтобы стал виден последний айтем
  /// </summary>
  public class ScrollToBottomOnLoadBehavior : Behavior<ListView>
  {
    protected override void OnAttached() // это стандартное место
    {
      AssociatedObject.Loaded +=AssociatedObjectOnLoaded;  // а здесь подписываюсь именно на то событие, по которому должно выполняться действие
    }

    /// <summary>
    ///  действия, которые я хочу чтобы выполнились
    /// </summary>
    private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
      AssociatedObject.ScrollIntoView(AssociatedObject.Items[AssociatedObject.Items.Count - 1]);
    }
  }
}