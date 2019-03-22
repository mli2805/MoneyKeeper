using System;
using System.Composition;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Keeper.Models.Shell;
using Keeper.Utils.DbInputOutput.CompositeTasks;
using Keeper.Utils.Dialogs;
using Keeper.ViewModels.Shell.MainMenuActions;

namespace Keeper.ViewModels.Shell
{
    [Export]
    public class MainMenuViewModel : Screen
    {
        private readonly DbLoadResult _loadResult;
        private readonly MainMenuExecutor _mainMenuExecutor;
        private readonly ShellModel _shellModel;

        public bool IsDbLoadingFailed { get; set; }

        [ImportingConstructor]
        public MainMenuViewModel(DbLoadResult loadResult,
            MainMenuExecutor mainMenuExecutor, ShellModel shellModel,
                                 IMessageBoxer messageBoxer)
        {
            _loadResult = loadResult; // в конструкторе DbLoadResult происходит загрузка БД

            IsDbLoadingFailed = _loadResult.Db == null;
            if (IsDbLoadingFailed)
            {
                messageBoxer.Show(_loadResult.Explanation + "\nApplication will be closed!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _mainMenuExecutor = mainMenuExecutor;
            _shellModel = shellModel;

            _shellModel.IsDbChanged = false;
            messageBoxer.DropEmptyBox();
        }

        public async void ActionMethod(MainMenuAction action)
        {
            if (_shellModel.CurrentAction == MainMenuAction.QuitApplication) return; // отсеиваем двойное нажатие
            _shellModel.CurrentAction = action;

            if (_shellModel.MainMenuDictionary.Actions[action].IsAsync)
                await Task.Run(() => _mainMenuExecutor.Execute(action));
            else
                _mainMenuExecutor.Execute(action);

            if (_mainMenuExecutor.IsDbChanged)
            {
                _shellModel.IsDbChanged = true;
            }
            Console.WriteLine($"Done {DateTime.Now}");
            _shellModel.CurrentAction = MainMenuAction.DoNothing;
        }

    }
}
