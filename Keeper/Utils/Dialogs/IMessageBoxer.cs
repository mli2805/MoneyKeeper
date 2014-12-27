using System.Windows;

namespace Keeper.Utils.Dialogs
{
	public interface IMessageBoxer
	{
		MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage);
	  void DropEmptyBox();
	}
}