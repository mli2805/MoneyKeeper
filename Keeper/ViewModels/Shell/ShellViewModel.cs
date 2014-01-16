using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Models;
using Keeper.Properties;
using Keeper.Utils.Balances;
using Keeper.Utils.Common;
using Keeper.Utils.DbInputOutput;
using Keeper.Utils.DbInputOutput.CompositeTasks;
using Keeper.Utils.DbInputOutput.TxtTasks;

namespace Keeper.ViewModels.Shell
{
  [Export(typeof(IShell))] // это для загрузчика, который ищет главное окно проги
  [Export(typeof(ShellViewModel))]
  [Shared] // это для класса Account чтобы засунуть в свойство SelectedAccountInShell 
  public class ShellViewModel : Screen, IShell
  {
    [Import]
    public IWindowManager WindowManager { get; set; }

    public MainMenuViewModel MainMenuViewModel { get; set; }
    public AccountForestViewModel AccountForestViewModel { get; set; }
    public BalanceListViewModel BalanceListViewModel { get; set; }
    public TwoSelectorsViewModel TwoSelectorsViewModel { get; set; }
    public StatusBarViewModel StatusBarViewModel { get; set; }

    private readonly ShellModel _shellModel;
    private KeeperDb _db;
    readonly DbLoadResult _loadResult;
    private readonly List<Screen> _launchedForms = new List<Screen>();
    private bool _isDbLoadingSuccessed;

    private readonly IDbToTxtSaver _txtSaver;
    private readonly DbBackuper _backuper;
    readonly IDbFromTxtLoader _dbFromTxtLoader;

    [ImportingConstructor]
    public ShellViewModel(ShellModel shellModel, KeeperDb db, DbLoadResult loadResult, 
       IDbToTxtSaver txtSaver, DbBackuper backuper, IDbFromTxtLoader dbFromTxtLoader)
    {
      _shellModel = shellModel;
      _db = db;
      _loadResult = loadResult;

      _isDbLoadingSuccessed = _db != null;
      if (!_isDbLoadingSuccessed)
      {
        MessageBox.Show(_loadResult.Explanation + "\nApplication will be closed!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }
      _isDbLoadingSuccessed = true;

      MainMenuViewModel = IoC.Get<MainMenuViewModel>();

      _txtSaver = txtSaver;
      _backuper = backuper;
      _dbFromTxtLoader = dbFromTxtLoader;

      AccountForestViewModel = IoC.Get<AccountForestViewModel>();
      TwoSelectorsViewModel = IoC.Get<TwoSelectorsViewModel>();
      BalanceListViewModel = IoC.Get<BalanceListViewModel>();
      StatusBarViewModel = IoC.Get<StatusBarViewModel>();
    }

    protected override void OnViewLoaded(object view)
    {
      if (!_isDbLoadingSuccessed)
      {
        TryClose();
        return;
      }
      DisplayName = "Keeper (c) 2012-13";
      StatusBarViewModel.MyStatusBarModel.Message = DateTime.Today.ToString("dddd , dd MMMM yyyy");

      if (!ShowLogonForm()) TryClose();
    }

    public override async void CanClose(Action<bool> callback)
    {
      if (_isDbLoadingSuccessed)
      {
        foreach (var launchedForm in _launchedForms.Where(launchedForm => launchedForm.IsActive))
          launchedForm.TryClose();
        await Task.Run(() => SerializeWithProgressBar());
        //        await Task.Run(() => MakeBackupWithProgressBar());
        StatusBarViewModel.MyStatusBarModel.Item0 = "Idle";
        StatusBarViewModel.MyStatusBarModel.ProgressBarVisibility = Visibility.Collapsed;
      }
      callback(true);
    }


    private void SerializeWithProgressBar()
    {
      StatusBarViewModel.MyStatusBarModel.Item0 = "Сохранение данных на диск";
      StatusBarViewModel.MyStatusBarModel.ProgressBarVisibility = Visibility.Visible;
      new DbSerializer().EncryptAndSerialize(_db, Path.Combine(Settings.Default.DbPath, Settings.Default.DbxFile));
    }

    //    private void RefreshBalanceList()
    //    {
    //       по возвращении на главную форму пересчитать остаток/оборот по выделенному счету/категории
    //      var period = SelectedAccountInShell.Is("Мои")
    //                     ? new Period(new DateTime(0), new DayProcessor(BalanceDate).AfterThisDay())
    //                     : PaymentsPeriod;
    //      _balanceCalculator.CountBalances(SelectedAccountInShell, period, BalanceList);
    //      if (SelectedAccountInShell.Is("Мои")) BalanceDate = BalanceDate;
    //      else PaymentsPeriod = PaymentsPeriod;
    //    }

    // сохраняет резервную копию БД в текстовом виде , в шифрованный zip
    private void MakeBackupWithProgressBar()
    {
      StatusBarViewModel.MyStatusBarModel.Item0 = "Создание резервной копии БД";
      StatusBarViewModel.MyStatusBarModel.ProgressBarVisibility = Visibility.Visible;
      _backuper.MakeDbBackupCopy();
    }

    public bool ShowLogonForm()
    {
      var logonViewModel = new LogonViewModel("1");
      WindowManager.ShowDialog(logonViewModel);
      return logonViewModel.Result;
    }

    public void ProgramExit()
    {
      TryClose();
    }


  }
}
