using System.Windows;

namespace Keeper.Utils.Dialogs
{
	interface IMessageBoxer 
  {
		void Show(string messageBoxText, string caption, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage);
	}
}