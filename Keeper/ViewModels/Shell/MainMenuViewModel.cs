using System.Collections.Generic;
using System.Composition;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils.DbInputOutput.CompositeTasks;
using Keeper.Utils.DbInputOutput.TxtTasks;
using Keeper.Utils.Diagram;

namespace Keeper.ViewModels.Shell
{
  [Export]
	public class MainMenuViewModel : Screen
  {
    [Import]
    private IWindowManager WindowManager { get; set; }

    private readonly KeeperDb _db;
    private readonly IDbToTxtSaver _txtSaver;
    private readonly DbBackuper _backuper;
    private readonly IDbFromTxtLoader _dbFromTxtLoader;
    private readonly DiagramDataFactory _diagramDataFactory;

    public string Message { get; set; }
    public string StatusBarItem0 { get; set; }
    public bool IsDbChanged { get; set; } // TwoWay binding needed
    private readonly List<Screen> _launchedForms = new List<Screen>();

    [ImportingConstructor]
    public MainMenuViewModel(KeeperDb db, IDbToTxtSaver txtSaver, DbBackuper backuper, 
                             IDbFromTxtLoader dbFromTxtLoader, DiagramDataFactory diagramDataFactory)
    {
      _db = db;
      _txtSaver = txtSaver;
      _backuper = backuper;
      _dbFromTxtLoader = dbFromTxtLoader;
      _diagramDataFactory = diagramDataFactory;
      WindowManager = new WindowManager();
    }
    /*
    #region меню Файл
    public async void SaveDatabase()
    {
      await Task.Run(() => SerializeWithProgressBar());
      StatusBarItem0 = "Idle";
      IsProgressBarVisible = Visibility.Collapsed;
    }

    public void LoadDatabase()
    {
      _db = new DbSerializer().DecryptAndDeserialize(Path.Combine(Settings.Default.DbPath, Settings.Default.DbxFile));
      InitVariablesToShowAccounts();
      InitBalanceControls();
    }

    public void ClearDatabase()
    {
      new DbCleaner().ClearAllTables(_db);
      InitVariablesToShowAccounts();
      SelectedAccountInShell = null;
    }

    public async void MakeDatabaseBackup()
    {
      await Task.Run(() => MakeBackupWithProgressBar());
      StatusBarItem0 = "Idle";
      IsProgressBarVisible = Visibility.Collapsed;
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
        InitVariablesToShowAccounts();
        InitBalanceControls();
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
    */
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

  }
}
