using System;
using System.Composition;

namespace Keeper.Utils.Dialogs
{
	[Export(typeof(IMyOpenFileDialog))]
	class MyOpenFileDialog : IMyOpenFileDialog
	{
		public string Show(string defaultExt, string filter, string defaultPath)
		{
		  var dialog = new Microsoft.Win32.OpenFileDialog
		                 {
		                   DefaultExt = defaultExt, Filter = filter, InitialDirectory = defaultPath
		                 };
		  bool? dialogResult;
		  try
		  {
        dialogResult = dialog.ShowDialog();  
		  }
		  catch (Exception)
		  {
		    return "";
		  }
		  return dialogResult == true ? dialog.FileName : "";
		}
	}
}