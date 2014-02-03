using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Models;
using Keeper.Properties;
using Keeper.Utils;
using Keeper.Utils.CommonKeeper;
using Keeper.Utils.DbInputOutput;
using Keeper.Utils.DbInputOutput.CompositeTasks;
using Keeper.Utils.DbInputOutput.FileTasks;
using Keeper.Utils.DbInputOutput.TxtTasks;
using Keeper.Utils.Diagram;

namespace Keeper.ViewModels.Shell
{
  [Export]
  public class MainMenuViewModel : Screen
  {
    [Import]
    private IWindowManager WindowManager { get; set; }

    public MainMenuModel MyMainMenuModel { get; set; }
    private readonly DbLoadResult _loadResult;
    private KeeperDb _db;
    private readonly IDbToTxtSaver _txtSaver;
    private readonly DbBackuper _backuper;
    private readonly IDbFromTxtLoader _dbFromTxtLoader;
    private readonly DbCleaner _dbCleaner;
    private readonly DiagramDataFactory _diagramDataFactory;
    private readonly OldTransactionsReorderer _oldTransactionsReorderer;

    public bool IsDbLoadingFailed { get; set; }
    public bool IsAuthorizationFailed { get; set; }
    private readonly List<Screen> _launchedForms = new List<Screen>();
    private bool _isExitPreparationDone;
    public bool IsExitPreparationDone
    {
      get { return _isExitPreparationDone; }
      set
      {
        if (value.Equals(_isExitPreparationDone)) return;
        _isExitPreparationDone = value;
        NotifyOfPropertyChange(() => IsExitPreparationDone);
      }
    }

    [ImportingConstructor]
    public MainMenuViewModel(DbLoadResult loadResult, KeeperDb db, ShellModel shellModel, IDbToTxtSaver txtSaver, DbBackuper backuper,
                             IDbFromTxtLoader dbFromTxtLoader, DbCleaner dbCleaner, DiagramDataFactory diagramDataFactory, OldTransactionsReorderer oldTransactionsReorderer)
    {
      _loadResult = loadResult;
      IsDbLoadingFailed = _loadResult.Db == null;
      if (IsDbLoadingFailed)
      {
        MessageBox.Show(_loadResult.Explanation + "\nApplication will be closed!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      MyMainMenuModel = shellModel.MyMainMenuModel;

      _db = db;
      _txtSaver = txtSaver;
      _backuper = backuper;
      _dbFromTxtLoader = dbFromTxtLoader;
      _dbCleaner = dbCleaner;
      _diagramDataFactory = diagramDataFactory;
      _oldTransactionsReorderer = oldTransactionsReorderer;
      WindowManager = new WindowManager();
    }

    #region меню Файл
    public async void SaveDatabase()
    {
      MyMainMenuModel.Action = Actions.SaveDatabase;
      await Task.Run(() => new DbSerializer().EncryptAndSerialize(_db, Path.Combine(Settings.Default.DbPath, Settings.Default.DbxFile)));
      Task.WaitAll();
      MyMainMenuModel.Action = Actions.Idle;
    }

    public void DeserializeWithoutReturn()
    {
      _db = new DbSerializer().DecryptAndDeserialize(Path.Combine(Settings.Default.DbPath, Settings.Default.DbxFile));
    }

    public async void LoadDatabase()
    {
      MyMainMenuModel.Action = Actions.LoadDatabase;
      await Task.Run(() => DeserializeWithoutReturn());
      Task.WaitAll();
      MyMainMenuModel.Action = Actions.RefreshBalanceList;
    }

    public void ClearDatabase()
    {
      _dbCleaner.ClearAllTables(_db);
      MyMainMenuModel.Action = Actions.CleanDatabase;
    }

    public async void MakeDatabaseBackup()
    {
      MyMainMenuModel.Action = Actions.SaveDatabase;
      await Task.Run(() => _backuper.MakeDbBackupCopy());
      Task.WaitAll();
      MyMainMenuModel.Action = Actions.Idle;
    }

    public void ExportDatabaseToTxt()
    {
      _txtSaver.SaveDbInTxt();
    }

    public void LoadFromWithoutReturn()
    {
      var result = _dbFromTxtLoader.LoadDbFromTxt(Settings.Default.TemporaryTxtDbPath);
      if (result.Code != 0) MessageBox.Show(result.Explanation);
      else
      {
        _db = result.Db;
      }
    }

    public async void ImportDatabaseFromTxt()
    {
      MyMainMenuModel.Action = Actions.LoadFromFiles;
      await Task.Run(() => LoadFromWithoutReturn());
      Task.WaitAll();
      MyMainMenuModel.Action = Actions.RefreshBalanceList;
    }

    public async void RemoveExtraBackups()
    {
      MyMainMenuModel.Action = Actions.RemoveIdenticalBackups;
      await Task.Run(() =>new DbBackupOrganizer().RemoveIdenticalBackups());
      Task.WaitAll();
      MyMainMenuModel.Action = Actions.Idle;
    }

    #endregion

    #region меню Формы

    public void ShowTransactionsForm()
    {
      MyMainMenuModel.Action = Actions.InputTransactions;
      var tvm = IoC.Get<TransactionViewModel>();
      WindowManager.ShowDialog(tvm);
      if (tvm.ShouldChangesBeSerialized)
      {
        SaveDatabase();
        MyMainMenuModel.Action = Actions.RefreshBalanceList;
      }
    }

    public void ShowCurrencyRatesForm()
    {
      MyMainMenuModel.Action = Actions.InputRates;
      WindowManager.ShowDialog(IoC.Get<RatesViewModel>());
      SaveDatabase();
      MyMainMenuModel.Action = Actions.RefreshBalanceList;
    }

    public void ShowArticlesAssociationsForm()
    {
      MyMainMenuModel.Action = Actions.InputAssociates;
      WindowManager.ShowDialog(IoC.Get<ArticlesAssociationsViewModel>());
      SaveDatabase();
    }

    public void ShowMonthAnalisysForm()
    {
      MyMainMenuModel.Action = Actions.ShowAnalisys;
      WindowManager.ShowDialog(IoC.Get<MonthAnalisysViewModel>());
      MyMainMenuModel.Action = Actions.Idle;
    }

    public void ShowDepositsForm()
    {
      foreach (var launchedForm in _launchedForms)
        if (launchedForm is DepositViewModel && launchedForm.IsActive) launchedForm.TryClose();

      var depositsForm = IoC.Get<DepositsViewModel>();

      _launchedForms.Add(depositsForm);
      WindowManager.ShowWindow(depositsForm);
    }

    #endregion

    #region меню Диаграммы

    public void ShowDailyBalancesDiagram()
    {
      OpenDiagramForm(_diagramDataFactory.DailyBalancesCtor());
    }

    public void ShowRatesDiagram()
    {
      OpenDiagramForm(_diagramDataFactory.RatesCtor());
    }

    public void ShowMonthlyResultDiagram()
    {
      OpenDiagramForm(_diagramDataFactory.MonthlyResultsDiagramCtor());
    }

    public void ShowMonthlyIncomeDiagram()
    {
      OpenDiagramForm(_diagramDataFactory.MonthlyIncomesDiagramCtor());
    }

    public void ShowMonthlyOutcomeDiagram()
    {
      OpenDiagramForm(_diagramDataFactory.MonthlyOutcomesDiagramCtor());
    }

    public void ShowAverageSignificancesDiagram()
    {
      OpenDiagramForm(_diagramDataFactory.AverageSignificancesDiagramCtor());
    }

    private void OpenDiagramForm(DiagramData diagramData)
    {
      var diagramForm = new DiagramViewModel(diagramData);
      _launchedForms.Add(diagramForm);
      WindowManager.ShowWindow(diagramForm);
    }
    #endregion

    public void TempItem()
    {
//      var diagramData = _diagramDataFactory.MonthlyResultsDiagramCtor();
//      var diagramOxyplotViewModel = new DiagramOxyplotViewModel(diagramData);
//      WindowManager.ShowDialog(diagramOxyplotViewModel);

      _oldTransactionsReorderer.MarkOldAvtoExpenseBySpecialArticle();
    }

    public void ShowToDoForm()
    {
      var toDoForm = new ToDoViewModel();
      _launchedForms.Add(toDoForm);
      WindowManager.ShowWindow(toDoForm);
    }

    public bool ShowLogonForm()
    {
      IsAuthorizationFailed = true;
      var logonViewModel = new LogonViewModel("1");
      WindowManager.ShowDialog(logonViewModel);
      IsAuthorizationFailed = !logonViewModel.Result;
      return logonViewModel.Result;
    }

    public void CloseAllLaunchedForms()
    {
      foreach (var launchedForm in _launchedForms.Where(launchedForm => launchedForm.IsActive))
        launchedForm.TryClose();
    }

    public void ProgramExit()
    {
      MadeExitPreparationsAsynchronously();
    }

    public async void MadeExitPreparationsAsynchronously()
    {
      MyMainMenuModel.Action = Actions.PrepareExit;
      CloseAllLaunchedForms();
      await Task.Run(() => new DbSerializer().EncryptAndSerialize(_db, Path.Combine(Settings.Default.DbPath, Settings.Default.DbxFile)));
      await Task.Run(() => _backuper.MakeDbBackupCopy());
      Task.WaitAll();
      MyMainMenuModel.Action = Actions.Idle;
      IsExitPreparationDone = true;
    }

  }
}
