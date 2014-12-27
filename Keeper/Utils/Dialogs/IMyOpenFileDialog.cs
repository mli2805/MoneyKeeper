namespace Keeper.Utils.Dialogs
{
	public interface IMyOpenFileDialog {
		string Show(string defaultExt, string filter, string defaultPath);
	}
}