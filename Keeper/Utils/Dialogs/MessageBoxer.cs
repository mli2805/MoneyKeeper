using System.Composition;
using System.Windows;

namespace Keeper.Utils.Dialogs
{
  [Export(typeof (IMessageBoxer))]
  [Shared]
  internal class MessageBoxer : IMessageBoxer
  {
    private bool _isBeforeNormal = true;

    public MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton messageBoxButton,
                                 MessageBoxImage messageBoxImage)
    {
      if (_isBeforeNormal)
      {
        MessageBox.Show("");
        _isBeforeNormal = false;
      }
      return MessageBox.Show(messageBoxText, caption, messageBoxButton, messageBoxImage);
    }

    public void DropEmptyBox()
    {
      _isBeforeNormal = false;
    }
  }
}