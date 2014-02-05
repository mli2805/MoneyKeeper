using System.Composition;
using System.Windows;

namespace Keeper.Utils.Dialogs
{
	[Export(typeof(IMessageBoxer))]
	[Shared]
	class MessageBoxer : IMessageBoxer
	{
		public bool IsBeforeNormal= true;
		public MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage)
		{
			if (IsBeforeNormal)
			{
				MessageBox.Show("");
				IsBeforeNormal = false;
			}
			return MessageBox.Show(messageBoxText, caption, messageBoxButton, messageBoxImage);
		}
	}
}