using System.Composition;
using System.Threading.Tasks;
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
                case MainMenuAction.SaveDatabase: SaveDatabase(); break;
                case MainMenuAction.LoadDatabase: LoadDatabase(); break;
                case MainMenuAction.ClearDatabase: ClearDatabase(); break;
                case MainMenuAction.MakeDatabaseBackup: MakeDatabaseBackup(); break;
                case MainMenuAction.ExportDatabaseToTxt: ExportDatabaseToTxt(); break;
                case MainMenuAction.ImportDatabaseFromTxt: ImportDatabaseFromTxt(); break;
                case MainMenuAction.RemoveExtraBackups: RemoveExtraBackups(); break;
                default: break;
            }
        }

        public async void SaveDatabase()
        {
            await Task.Run(() => new DbSerializer().EncryptAndSerialize(_db, _mySettings.GetCombinedSetting("DbFileFullPath")));
            Task.WaitAll();
        }

        public void DeserializeWithoutReturn()
        {
            _db = new DbSerializer().DecryptAndDeserialize(_mySettings.GetCombinedSetting("DbFileFullPath"));
        }

        public async void LoadDatabase()
        {
            await Task.Run(() => DeserializeWithoutReturn());
            Task.WaitAll();
        }

        public void ClearDatabase()
        {
            _dbCleaner.ClearAllTables(_db);
        }

        public async void MakeDatabaseBackup()
        {
            await Task.Run(() => _dbBackuper.MakeDbTxtCopy());
            Task.WaitAll();
        }
        public void ExportDatabaseToTxt()
        {
            _dbToTxtSaver.SaveDbInTxt();
        }
        public void LoadFromWithoutReturn()
        {
            var result = _dbFromTxtLoader.LoadDbFromTxt((string)_mySettings.GetSetting("TemporaryTxtDbPath"));
            if (result.Code != 0) MessageBox.Show(result.Explanation);
            else
            {
                _db = result.Db;
            }
        }
        public async void ImportDatabaseFromTxt()
        {
            await Task.Run(() => LoadFromWithoutReturn());
            Task.WaitAll();
        }

        public async void RemoveExtraBackups()
        {
            await Task.Run(() => new DbBackupOrganizer().RemoveIdenticalBackups());
            Task.WaitAll();
        }

    }
}
