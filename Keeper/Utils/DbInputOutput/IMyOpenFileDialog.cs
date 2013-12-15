namespace Keeper.DbInputOutput
{
	interface IMyOpenFileDialog {
		string Show(string defaultExt, string filter, string filename, string defaultPath);
	}
}