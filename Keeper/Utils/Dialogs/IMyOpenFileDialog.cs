﻿namespace Keeper.Utils.Dialogs
{
	interface IMyOpenFileDialog {
		string Show(string defaultExt, string filter, string defaultPath);
	}
}