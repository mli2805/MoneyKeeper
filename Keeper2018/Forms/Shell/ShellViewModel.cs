using System;
using System.IO;
using System.Linq;
using System.Windows;
using Caliburn.Micro;

namespace Keeper2018
{
    public class ShellViewModel : Screen, IShell
    {
        public MainMenuViewModel MainMenuViewModel { get; set; }
        public AccountTreeViewModel AccountTreeViewModel { get; }
        public BalanceOrTrafficViewModel BalanceOrTrafficViewModel { get; }
        public TwoSelectorsViewModel TwoSelectorsViewModel { get; }

        private readonly KeeperDb _keeperDb;
        private readonly DbLoader _dbLoader;
        private readonly TransModel _transModel;
        public ShellPartsBinder ShellPartsBinder { get; }
        private bool _dbLoaded;

        public ShellViewModel(KeeperDb keeperDb, DbLoader dbLoader, ShellPartsBinder shellPartsBinder,
            MainMenuViewModel mainMenuViewModel, AccountTreeViewModel accountTreeViewModel, TransModel transModel,
            BalanceOrTrafficViewModel balanceOrTrafficViewModel, TwoSelectorsViewModel twoSelectorsViewModel)
        {
            MainMenuViewModel = mainMenuViewModel;
            AccountTreeViewModel = accountTreeViewModel;
            BalanceOrTrafficViewModel = balanceOrTrafficViewModel;
            TwoSelectorsViewModel = twoSelectorsViewModel;

            _keeperDb = keeperDb;
            _dbLoader = dbLoader;
            _transModel = transModel;
            ShellPartsBinder = shellPartsBinder;
        }

        protected override async void OnViewLoaded(object view)
        {
            DisplayName = "Keeper 2018";
            _dbLoaded = await _dbLoader.Load();
            if (!_dbLoaded)
            {
                TryClose();
                return;
            }

            _dbLoader.ExpandBinToDb(_keeperDb);

            var account = _keeperDb.AccountsTree.First(r => r.Name == "���");
            account.IsSelected = true;
            ShellPartsBinder.SelectedAccountModel = account;
        }

        public override async void CanClose(Action<bool> callback)
        {
            if (_dbLoaded)
            {
                ShellPartsBinder.FooterVisibility = Visibility.Visible;
                _keeperDb.FlattenAccountTree();
                await DbSerializer.Serialize(_keeperDb.Bin);

                if (_transModel.IsCollectionChanged)
                {
                    var unused1 = await _keeperDb.SaveAllToNewTxtAsync();
                    var unused2 = await DbTxtSaver.ZipTxtDbAsync();
                    DeleteTxtFiles();
                }
            
                ShellPartsBinder.FooterVisibility = Visibility.Collapsed;
            }
            base.CanClose(callback);
        }

        private void DeleteTxtFiles()
        {
            var backupPath = DbIoUtils.GetBackupPath();
            if (!Directory.Exists(backupPath)) return;
            var filenames = Directory.GetFiles(backupPath, "*.txt"); // note: this does not recurse directories! 
            foreach (var filename in filenames)
                File.Delete(filename);
        }

    }
}