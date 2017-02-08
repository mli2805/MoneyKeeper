using System;
using System.ComponentModel;
using System.Composition;
using Caliburn.Micro;
using Keeper.Models.Shell;
using Keeper.ViewModels.Shell.MainMenuActions;
using Keeper.ViewModels.SingleViews;

namespace Keeper.ViewModels.Shell
{
    [Export(typeof(IShell))] // это для загрузчика, который ищет главное окно проги
    [Export(typeof(ShellViewModel))]
    public class ShellViewModel : Screen, IShell
    {
        private readonly ShellModel _shellModel;
        public MainMenuViewModel MainMenuViewModel { get; set; }
        public AccountForestViewModel AccountForestViewModel { get; set; }
        public BalanceListViewModel BalanceListViewModel { get; set; }
        public TwoSelectorsViewModel TwoSelectorsViewModel { get; set; }
        public StatusBarViewModel StatusBarViewModel { get; set; }

        private IWindowManager WindowManager { get; set; }

        [ImportingConstructor]
        public ShellViewModel(ShellModel shellModel)
        {
            _shellModel = shellModel;
            _shellModel.PropertyChanged += _shellModel_PropertyChanged;
            MainMenuViewModel = IoC.Get<MainMenuViewModel>();
            if (MainMenuViewModel.IsDbLoadingFailed) return;

            AccountForestViewModel = IoC.Get<AccountForestViewModel>();
            TwoSelectorsViewModel = IoC.Get<TwoSelectorsViewModel>();
            BalanceListViewModel = IoC.Get<BalanceListViewModel>();
            StatusBarViewModel = IoC.Get<StatusBarViewModel>();

            WindowManager = new WindowManager();
        }

        private void _shellModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsExitPreparationDone") 
                if (_shellModel.IsExitPreparationDone)
                    TryClose();
        }

        protected override void OnViewLoaded(object view)
        {
            if (MainMenuViewModel.IsDbLoadingFailed)
            {
                TryClose();
                return;
            }
            DisplayName = "Keeper (c) 2012-17";

            if (!ShowLogonForm()) TryClose();
        }

        private bool ShowLogonForm()
        {
            _shellModel.IsAuthorizationFailed = true;
            var logonViewModel = new LogonViewModel("1");
            WindowManager.ShowDialog(logonViewModel);
            _shellModel.IsAuthorizationFailed = !logonViewModel.Result;
            return logonViewModel.Result;
        }


        public override void CanClose(Action<bool> callback)
        {
            if (MainMenuViewModel.IsDbLoadingFailed || _shellModel.IsAuthorizationFailed || _shellModel.IsExitPreparationDone)
            {
                callback(true);
                return;
            }

            MainMenuViewModel.ActionMethod(MainMenuAction.QuitApplication);
            callback(false);
        }
    }
}
