using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Keeper.Behaviors.ListViewBehaviors
{
    /// <summary>
    /// ���� Behavior ��������� �� ListView (�.�. AssociatedObject ��� ListView)
    /// 
    /// � ������ ��������� Behavior ������������� �� ����� OnLoaded ������� � ������� �� ���������������
    /// ������ ����� ListView ���������� (Loaded) ��������� ��� ��� 
    /// � ������� ������� ����������� ListView ����� ���� ����� ��������� �����
    /// </summary>
    public class ScrollToPreviousExitPointOrBottomOnLoadBehavior : Behavior<ListView>
    {
        protected override void OnAttached() // ��� ����������� �����
        {
            AssociatedObject.Loaded += AssociatedObjectOnLoaded;  // � ����� ������������ ������ �� �� �������, �� �������� ������ ����������� ��������
//            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        /// <summary>
        ///  ��������, ������� � ���� ����� �����������
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
                // ���� ��� �������� �� ������������ �� � ��������� ������, �� ���������� �� ��������� � �������� �������
                int itemNumber = AssociatedObject.SelectedIndex + 10;
                if (itemNumber > AssociatedObject.Items.Count - 1) itemNumber = AssociatedObject.Items.Count - 1;
                AssociatedObject.ScrollIntoView(AssociatedObject.Items[itemNumber]);
            }
        }
    }
}