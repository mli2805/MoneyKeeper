using System.Composition;
using System.Windows;
using Keeper.DomainModel.DbTypes;
using Keeper.Utils.Common;
using Keeper.Utils.CommonKeeper;
using Keeper.Utils.DbInputOutput;
using Keeper.Utils.DbInputOutput.CompositeTasks;
using Keeper.Utils.DbInputOutput.FileTasks;
using Keeper.Utils.DbInputOutput.TxtTasks;

namespace Keeper.ViewModels.Shell.MainMenuActions
{
    [Export]
    public class MenuFileExecutor
    {
        private KeeperDb _db;
        private readonly MySettings _mySettings;
        private readonly DbCleaner _dbCleaner;
        private readonly DbBackuper _dbBackuper;
        private readonly IDbToTxtSaver _dbToTxtSaver;
        private readonly IDbFromTxtLoader _dbFromTxtLoader;

        [ImportingConstructor]
        public MenuFileExecutor(KeeperDb db, MySettings mySettings, 
            DbCleaner dbCleaner, DbBackuper dbBackuper, IDbToTxtSaver dbToTxtSaver, IDbFromTxtLoader dbFromTxtLoader)
        {
            _db = db;
            _mySettings = mySettings;
            _dbCleaner = dbCleaner;
            _dbBackuper = dbBackuper;
            _dbToTxtSaver = dbToTxtSaver;
            _dbFromTxtLoader = dbFromTxtLoader;
        }

        public void Execute(MainMenuAction action)
        {
            switch (action)
            {
                case MainMenuAction.SaveDatabase:
                    new DbSerializer().EncryptAndSerialize(_db, _mySettings.GetCombinedSetting("DbFileFullPath"));
                    break;
                case MainMenuAction.LoadDatabase:
                    _db = new DbSerializer().DecryptAndDeserialize(_mySettings.GetCombinedSetting("DbFileFullPath"));
                    break;
                case MainMenuAction.ClearDatabase:
                    _dbCleaner.ClearAllTables(_db);
                    break;
                case MainMenuAction.MakeDatabaseBackup:
                    _dbBackuper.MakeDbTxtCopy();
                    break;
                case MainMenuAction.ExportDatabaseToTxt:
                    _dbToTxtSaver.SaveDbInTxt();
                    break;
                case MainMenuAction.ImportDatabaseFromTxt:
                    ImportDatabaseFromTxt();
                    break;
                case MainMenuAction.RemoveExtraBackups:
                    new DbBackupOrganizer().RemoveIdenticalBackups();
                    break;
                case MainMenuAction.RemoveAllNonFirstInMonth:
                    new DbBackupOrganizer().RemoveAllNonFirstInMonth();
                    break;

            }
        }

        private void ImportDatabaseFromTxt()
        {
            var result = _dbFromTxtLoader.LoadDbFromTxt((string)_mySettings.GetSetting("TemporaryTxtDbPath"));
            if (result.Code != 0) MessageBox.Show(result.Explanation);
            else
            {
                _db = result.Db;
            }
        }
    }
}
