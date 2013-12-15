using System.Windows;

namespace Keeper.DbInputOutput
{
	interface IMessageBoxer {
		void Show(string messageBoxText, string caption, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage);
	}
}