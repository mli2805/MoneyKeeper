using System;
using System.Composition;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Keeper.ViewModels.Shell
{
  [Export(typeof(IShell))] // это для загрузчика, который ищет главное окно проги
  [Export(typeof(ShellViewModel))]
  public class ShellViewModel : Screen, IShell
  {
    public MainMenuViewModel MainMenuViewModel { get; set; }
    public AccountForestViewModel AccountForestViewModel { get; set; }
    public BalanceListViewModel BalanceListViewModel { get; set; }
    public TwoSelectorsViewModel TwoSelectorsViewModel { get; set; }
    public StatusBarViewModel StatusBarViewModel { get; set; }

    [ImportingConstructor]
    public ShellViewModel()
    {
      MainMenuViewModel = IoC.Get<MainMenuViewModel>();
      if (MainMenuViewModel.IsDbLoadingFailed) return;
      MainMenuViewModel.PropertyChanged += MainMenuViewModelPropertyChanged;

      AccountForestViewModel = IoC.Get<AccountForestViewModel>();
      TwoSelectorsViewModel = IoC.Get<TwoSelectorsViewModel>();
      BalanceListViewModel = IoC.Get<BalanceListViewModel>();
      StatusBarViewModel = IoC.Get<StatusBarViewModel>();
    }

    void MainMenuViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "IsExitRequired")
        if (MainMenuViewModel.IsExitRequired) TryClose();
    }

    protected override void OnViewLoaded(object view)
    {
      if (MainMenuViewModel.IsDbLoadingFailed)
      {
        TryClose();
        return;
      }
      DisplayName = "Keeper (c) 2012-13";

      if (!MainMenuViewModel.ShowLogonForm()) TryClose();
    }

    public override void CanClose(Action<bool> callback)
    {
      if (!MainMenuViewModel.IsDbLoadingFailed)
      {
        MainMenuViewModel.CloseAllLaunchedForms();
//        await Task.Run(() => MainMenuViewModel.SaveDatabase());
        MainMenuViewModel.SaveDatabase();
        MainMenuViewModel.MakeDatabaseBackup();
      }
      callback(true);
    }
  }
}
