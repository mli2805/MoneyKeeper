using System.Windows;

namespace Keeper.Utils.Dialogs
{
	interface IMessageBoxer {
		MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage);
	}
}