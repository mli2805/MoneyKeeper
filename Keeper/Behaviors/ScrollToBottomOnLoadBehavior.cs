using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Keeper.Behaviors
{
  /// <summary>
  /// ���� Behavior ��������� �� ListView (�.�. AssociatedObject ��� ListView)
  /// 
  /// � ������ ��������� Behavior ������������� �� ����� OnLoaded ������� � ������� �� ���������������
  /// ������ ����� ListView ���������� (Loaded) ��������� ��� ��� 
  /// � ������� ������� ����������� ListView ����� ���� ����� ��������� �����
  /// </summary>
  public class ScrollToBottomOnLoadBehavior : Behavior<ListView>
  {
    protected override void OnAttached() // ��� ����������� �����
    {
      AssociatedObject.Loaded +=AssociatedObjectOnLoaded;  // � ����� ������������ ������ �� �� �������, �� �������� ������ ����������� ��������
    }

    /// <summary>
    ///  ��������, ������� � ���� ����� �����������
    /// </summary>
    private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
      AssociatedObject.ScrollIntoView(AssociatedObject.Items[AssociatedObject.Items.Count - 1]);
    }
  }
}