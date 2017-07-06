using System;
using System.Composition;
using Microsoft.Win32;

namespace Keeper.Utils.Dialogs
{
	[Export(typeof(IMyOpenFileDialog))]
	class MyOpenFileDialog : IMyOpenFileDialog
	{
		public string Show(string defaultExt, string filter, string defaultPath)
		{
		  var dialog = new OpenFileDialog
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