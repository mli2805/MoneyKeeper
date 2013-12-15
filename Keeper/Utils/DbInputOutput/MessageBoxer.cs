using System.Composition;
using System.Windows;

namespace Keeper.DbInputOutput
{
	[Export(typeof(IMessageBoxer))]
	[Shared]
	class MessageBoxer : IMessageBoxer
	{
		bool mIsFirstTime= true;
		public void Show(string messageBoxText, string caption, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage)
		{
			if (mIsFirstTime)
			{
				MessageBox.Show("");
				mIsFirstTime = false;
			}
			MessageBox.Show(messageBoxText, caption, messageBoxButton, messageBoxImage);
		}
	}
}