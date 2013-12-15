using System.Composition;

namespace Keeper.Utils.Dialogs
{
	[Export(typeof(IMyOpenFileDialog))]
	class MyOpenFileDialog : IMyOpenFileDialog
	{
		public string Show(string defaultExt, string filter, string filename, string defaultPath)
		{
			var dialog = new Microsoft.Win32.OpenFileDialog { DefaultExt = defaultExt, Filter = filter };
			var result = dialog.ShowDialog();
			filename = result == true ? dialog.FileName : defaultPath;
			return filename;
		}		
	}
}