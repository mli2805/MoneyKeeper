using System.Composition;
using System.Threading.Tasks;
using Caliburn.Micro;
using Keeper.DomainModel.DbTypes;
using Keeper.Models.Shell;
using Keeper.Utils.Common;
using Keeper.Utils.DbInputOutput;
using Keeper.Utils.DbInputOutput.CompositeTasks;

namespace Keeper.ViewModels.Shell.MainMenuActions
{
    [Export]
    public class MenuAboutExecutor
    {
        private readonly ShellModel _shellModel;
        private readonly KeeperDb _db;
        private readonly MySettings _mySettings;

        [ImportingConstructor]
        public MenuAboutExecutor(ShellModel shellModel, KeeperDb db, MySettings mySettings)
        {
            _shellModel = shellModel;
            _db = db;
            _mySettings = mySettings;
        }

        public void Execute(MainMenuAction action)
        {
            switch (action)
            {
                case MainMenuAction.ShowAboutForm: ShowAboutForm(); break;
                case MainMenuAction.QuitApplication: QuitApplication(); break;
                default: break;
            }
        }

        private void ShowAboutForm()
        {

        }
        private void QuitApplication()
        {
            MadeExitPreparationsAsynchronously();
        }

        public async void MadeExitPreparationsAsynchronously()
        {
//            _shellModel.CloseAllLaunchedForms();
            var pp = _mySettings.GetCombinedSetting("DbFileFullPath");

            await Task.Run(() => new DbSerializer().EncryptAndSerialize(_db, pp));
            if (_shellModel.IsDbChanged)
                await Task.Run(() => IoC.Get<DbBackuper>().MakeDbTxtCopy());
            Task.WaitAll();
            _shellModel.CurrentAction = MainMenuAction.DoNothing;
            _shellModel.IsExitPreparationDone = true;
        }


    }
}
