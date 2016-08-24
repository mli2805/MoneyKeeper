using System.Composition;
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
        private  void QuitApplication()
        {
            //            _shellModel.CloseAllLaunchedForms();
            SaveData();
            _shellModel.IsExitPreparationDone = true;
        }

        private void SaveData()
        {
            var pp = _mySettings.GetCombinedSetting("DbFileFullPath");
            new DbSerializer().EncryptAndSerialize(_db, pp);

            if (_shellModel.IsDbChanged)
                IoC.Get<DbBackuper>().MakeDbTxtCopy();
        }


    }
}
