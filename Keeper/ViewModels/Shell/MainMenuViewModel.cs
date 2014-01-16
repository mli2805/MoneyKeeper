using System;
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

    public string Message { get; set; }
    public string StatusBarItem0 { get; set; }
    private readonly List<Screen> _launchedForms = new List<Screen>();
    private bool _isExitRequired;
    public bool IsExitRequired
    {
      get { return _isExitRequired; }
      set
      {
        if (value.Equals(_isExitRequired)) return;
        _isExitRequired = value;
        NotifyOfPropertyChange(() => IsExitRequired);
      }
    }

    public bool IsDbLoadingFailed { get; set; }

    [ImportingConstructor]
    public MainMenuViewModel(DbLoadResult loadResult, KeeperDb db, ShellModel shellModel, IDbToTxtSaver txtSaver, DbBackuper backuper,
                             IDbFromTxtLoader dbFromTxtLoader, DbCleaner dbCleaner, DiagramDataFactory diagramDataFactory)
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
      WindowManager = new WindowManager();
    }

    #region меню Файл
    public void SaveDatabase()
    {
      new DbSerializer().EncryptAndSerialize(_db, Path.Combine(Settings.Default.DbPath, Settings.Default.DbxFile));
      MyMainMenuModel.Action = Actions.Idle;
    }

    public void LoadDatabase()
    {
      _db = new DbSerializer().DecryptAndDeserialize(Path.Combine(Settings.Default.DbPath, Settings.Default.DbxFile));
      MyMainMenuModel.Action = Actions.DatabaseLoaded;
    }

    public void ClearDatabase()
    {
      _dbCleaner.ClearAllTables(_db);
      MyMainMenuModel.Action = Actions.DatabaseCleaned;
    }

    public async void MakeDatabaseBackup()
    {
      _backuper.MakeDbBackupCopy();
      MyMainMenuModel.Action = Actions.Idle;
    }

    public void ExportDatabaseToTxt()
    {
      _txtSaver.SaveDbInTxt();
    }

    public void ImportDatabaseFromTxt()
    {
      var result = _dbFromTxtLoader.LoadDbFromTxt(Settings.Default.TemporaryTxtDbPath);
      if (result.Code != 0) MessageBox.Show(result.Explanation);
      else
      {
        _db = result.Db;
      }
    }

    public void RemoveExtraBackups()
    {
      String arcMessage = Message;
      Message = "Удаление идентичных резервных копий";
      new DbBackupOrganizer().RemoveIdenticalBackups();
      Message = arcMessage;
      StatusBarItem0 = "Готово";
    }

    #endregion
    #region меню Формы

    public void ShowTransactionsForm()
    {
      var arcMessage = Message;
      Message = "Input operations";
      WindowManager.ShowDialog(IoC.Get<TransactionViewModel>());
      Message = arcMessage;

      //      RefreshBalanceList();

      //      SerializeWithProgressBar();
      //      StatusBarItem0 = "Idle";
      //      IsProgressBarVisible = Visibility.Collapsed;
    }

    public void ShowCurrencyRatesForm()
    {
      var arcMessage = Message;
      Message = "Currency rates";
      WindowManager.ShowDialog(IoC.Get<RatesViewModel>());
      Message = arcMessage;

      //      RefreshBalanceList();

      //      SerializeWithProgressBar();
      //      StatusBarItem0 = "Idle";
      //      IsProgressBarVisible = Visibility.Collapsed;
    }

    public void ShowArticlesAssociationsForm()
    {
      var arcMessage = Message;
      Message = "Articles' associations";
      WindowManager.ShowDialog(IoC.Get<ArticlesAssociationsViewModel>());
      Message = arcMessage;
    }

    public void ShowMonthAnalisysForm()
    {
      var arcMessage = Message;
      Message = "MonthAnalisys";
      WindowManager.ShowDialog(IoC.Get<MonthAnalisysViewModel>());
      Message = arcMessage;
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
      var diagramData = _diagramDataFactory.MonthlyResultsDiagramCtor();
      var diagramOxyplotViewModel = new DiagramOxyplotViewModel(diagramData);
      WindowManager.ShowDialog(diagramOxyplotViewModel);
    }

    public void ShowToDoForm()
    {
      var toDoForm = new ToDoViewModel();
      _launchedForms.Add(toDoForm);
      WindowManager.ShowWindow(toDoForm);
    }

    public bool ShowLogonForm()
    {
      var logonViewModel = new LogonViewModel("1");
      WindowManager.ShowDialog(logonViewModel);
      return logonViewModel.Result;
    }

    public void CloseAllLaunchedForms()
    {
      foreach (var launchedForm in _launchedForms.Where(launchedForm => launchedForm.IsActive))
        launchedForm.TryClose();

    }

    public void ProgramExit()
    {
      IsExitRequired = true;
    }
  }
}
