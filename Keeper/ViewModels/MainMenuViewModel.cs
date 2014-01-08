using System;
using System.Composition;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Properties;
using Keeper.Utils.Common;
using Keeper.Utils.CommonKeeper;
using Keeper.Utils.DbInputOutput;
using Keeper.Utils.DbInputOutput.FileTasks;
using Keeper.Utils.Diagram;

namespace Keeper.ViewModels
{
  [Export]
	public class MainMenuViewModel : Screen
  {
	  private readonly DiagramDataFactory _diagramDataFactory;

	  [Import]
    private IWindowManager WindowManager { get; set; }

    [ImportingConstructor]
    public MainMenuViewModel(DiagramDataFactory diagramDataFactory)
    {
      _diagramDataFactory = diagramDataFactory;
      WindowManager = new WindowManager();
    }

    #region меню Файл
    public async void SaveDatabase()
    {
      await Task.Run(() => SerializeWithProgressBar());
      StatusBarItem0 = "Idle";
      IsProgressBarVisible = Visibility.Collapsed;
    }

    public void LoadDatabase()
    {
      Db = new DbSerializer().DecryptAndDeserialize(Path.Combine(Settings.Default.DbPath, Settings.Default.DbxFile));
      InitVariablesToShowAccounts();
      InitBalanceControls();
    }

    public void ClearDatabase()
    {
      new DbCleaner().ClearAllTables(Db);
      InitVariablesToShowAccounts();
      SelectedAccount = null;
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
      var result = mDbFromTxtLoader.LoadDbFromTxt(Settings.Default.TemporaryTxtDbPath);
      if (result.Code != 0) MessageBox.Show(result.Explanation);
      else
      {
        Db = result.Db;
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

    #region меню Формы

    public void ShowTransactionsForm()
    {
      var arcMessage = Message;
      Message = "Input operations";
      WindowManager.ShowDialog(IoC.Get<TransactionViewModel>());
      // по возвращении на главную форму пересчитать остаток/оборот по выделенному счету/категории
      var period = _openedAccountPage == 0 ? new Period(new DateTime(0), new DayProcessor(BalanceDate).AfterThisDay()) : PaymentsPeriod;
      _balanceCalculator.CountBalances(SelectedAccount, period, BalanceList);
      SerializeWithProgressBar();
      StatusBarItem0 = "Idle";
      IsProgressBarVisible = Visibility.Collapsed;
      if (OpenedAccountPage == 0) BalanceDate = BalanceDate; else PaymentsPeriod = PaymentsPeriod;
      Message = arcMessage;
    }

    public void ShowCurrencyRatesForm()
    {
      var arcMessage = Message;
      Message = "Currency rates";
      WindowManager.ShowDialog(IoC.Get<RatesViewModel>());
      SerializeWithProgressBar();
      StatusBarItem0 = "Idle";
      IsProgressBarVisible = Visibility.Collapsed;
      if (OpenedAccountPage == 0) BalanceDate = BalanceDate; else PaymentsPeriod = PaymentsPeriod;
      Message = arcMessage;
    }

    public void ShowArticlesAssociationsForm()
    {
      var arcMessage = Message;
      Message = "Articles' associations";
      WindowManager.ShowDialog(IoC.Get<ArticlesAssociationsViewModel>());
      Message = arcMessage;
    }

    public void ShowToDoForm()
    {
      var toDoForm = new ToDoViewModel();
      _launchedForms.Add(toDoForm);
      WindowManager.ShowWindow(toDoForm);
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
//      _launchedForms.Add(diagramForm);
      WindowManager.ShowWindow(diagramForm);
    }
    #endregion

    public void TempItem()
    {
      var diagramData = _diagramDataFactory.MonthlyResultsDiagramCtor();
      var diagramOxyplotViewModel = new DiagramOxyplotViewModel(diagramData);
      WindowManager.ShowDialog(diagramOxyplotViewModel);
    }

  }
}
