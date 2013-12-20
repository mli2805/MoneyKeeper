using System.Composition;
using System.Windows;

namespace Keeper.Utils.Dialogs
{
	[Export(typeof(IMessageBoxer))]
	[Shared]
	class MessageBoxer : IMessageBoxer
	{
		bool mIsFirstTime= true;
		public MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage)
		{
			if (mIsFirstTime)
			{
				MessageBox.Show("");
				mIsFirstTime = false;
			}
			return MessageBox.Show(messageBoxText, caption, messageBoxButton, messageBoxImage);
		}
	}
}