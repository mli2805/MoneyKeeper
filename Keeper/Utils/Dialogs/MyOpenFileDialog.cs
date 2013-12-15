using System.Composition;

namespace Keeper.Utils.Dialogs
{
	[Export(typeof(IMyOpenFileDialog))]
	class MyOpenFileDialog : IMyOpenFileDialog
	{
		public string Show(string defaultExt, string filter, string filename, string defaultPath)
		{
			// Create OpenFileDialog
			// Set filter for file extension and default file extension
			var dialog = new Microsoft.Win32.OpenFileDialog { DefaultExt = defaultExt, Filter = filter };
			var result = dialog.ShowDialog();
			// Get the selected file name and display in a TextBox
			filename = result == true ? dialog.FileName : defaultPath;
			return filename;
		}		
	}
}