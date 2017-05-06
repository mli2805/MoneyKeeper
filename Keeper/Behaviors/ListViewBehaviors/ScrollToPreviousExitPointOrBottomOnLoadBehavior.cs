using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Keeper.Behaviors.ListViewBehaviors
{
    /// <summary>
    /// Этот Behavior аттачится на ListView (т.е. AssociatedObject это ListView)
    /// 
    /// В момент аттаченья Behavior подписывается на метод OnLoaded объекта с котором он проассоциирован
    /// значит когда ListView загрузится (Loaded) вызовется мой код 
    /// в котором сказано проскролить ListView чтобы стал виден последний айтем
    /// </summary>
    public class ScrollToPreviousExitPointOrBottomOnLoadBehavior : Behavior<ListView>
    {
        protected override void OnAttached() // это стандартное место
        {
            AssociatedObject.Loaded += AssociatedObjectOnLoaded;  // а здесь подписываюсь именно на то событие, по которому должно выполняться действие
//            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        /// <summary>
        ///  действия, которые я хочу чтобы выполнились
        /// </summary>
        private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            ScrollIntoViewSelectedItem();
        }

        private void ScrollIntoViewSelectedItem()
        {
            if (AssociatedObject.Items.Count == 0) return;

            if (AssociatedObject.SelectedIndex == -1)
                AssociatedObject.ScrollIntoView(AssociatedObject.Items[AssociatedObject.Items.Count - 1]);
            else
            {
                // если при загрузке мы возвращаемся не к последней записи, то желательно ее поставить в середину таблицы
                int itemNumber = AssociatedObject.SelectedIndex + 10;
                if (itemNumber > AssociatedObject.Items.Count - 1) itemNumber = AssociatedObject.Items.Count - 1;
                AssociatedObject.ScrollIntoView(AssociatedObject.Items[itemNumber]);
            }
        }
    }
}