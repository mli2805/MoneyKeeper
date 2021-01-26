using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using KeeperDomain;

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
        private readonly TranModel _tranModel;
        public ShellPartsBinder ShellPartsBinder { get; }
        private bool _dbLoaded;

        public ShellViewModel(KeeperDb keeperDb, DbLoader dbLoader, ShellPartsBinder shellPartsBinder,
            MainMenuViewModel mainMenuViewModel, AccountTreeViewModel accountTreeViewModel, TranModel tranModel,
            BalanceOrTrafficViewModel balanceOrTrafficViewModel, TwoSelectorsViewModel twoSelectorsViewModel)
        {
            MainMenuViewModel = mainMenuViewModel;
            AccountTreeViewModel = accountTreeViewModel;
            BalanceOrTrafficViewModel = balanceOrTrafficViewModel;
            TwoSelectorsViewModel = twoSelectorsViewModel;

            _keeperDb = keeperDb;
            _dbLoader = dbLoader;
            _tranModel = tranModel;
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
            var account = _keeperDb.AccountsTree.First(r => r.Name == "Мои");
            account.IsSelected = true;
            ShellPartsBinder.SelectedAccountModel = account; 
            
//            var records = await ConvertInto2020Database();
        }

        // ReSharper disable once UnusedMember.Local
        private async Task<int> ConvertInto2020Database()
        {
            using (KeeperContext db = new KeeperContext())
            {
                db.Accounts.AddRange(_keeperDb.Bin.AccountPlaneList);
                return await db.SaveChangesAsync();
            }
        }

        public override async void CanClose(Action<bool> callback)
        {
            if (ShellPartsBinder.IsBusy) return;
            ShellPartsBinder.IsBusy = true;

            if (_dbLoaded)
            {
                ShellPartsBinder.FooterVisibility = Visibility.Visible;
                _keeperDb.FlattenAccountTree();

                var result3 = await BinSerializer.Serialize(_keeperDb.Bin);
                if (!result3.IsSuccess)
                {
                    MessageBox.Show(result3.Exception.Message);
                }

                if (_tranModel.IsCollectionChanged)
                {
                    var result = await _keeperDb.Bin.SaveAllToNewTxtAsync();
                    if (result.IsSuccess)
                    {
                        if (await DbTxtSaver.ZipTxtDbAsync())
                        {
                            var result2 = DbTxtSaver.DeleteTxtFiles();
                            if (!result2.IsSuccess)
                            {
                                MessageBox.Show(result2.Exception.Message);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show(result.Exception.Message);
                    }
                }

                ShellPartsBinder.FooterVisibility = Visibility.Collapsed;
            }

            base.CanClose(callback);
        }
    }
}