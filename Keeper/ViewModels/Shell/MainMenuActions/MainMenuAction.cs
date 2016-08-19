namespace Keeper.ViewModels.Shell.MainMenuActions
{
    public enum MainMenuAction : int
    {
        DoNothing = 0,

        SaveDatabase = 100,
        LoadDatabase,
        ClearDatabase,
        MakeDatabaseBackup,
        ExportDatabaseToTxt,
        ImportDatabaseFromTxt,
        RemoveExtraBackups,

        ShowTransactionsForm = 200,
        ShowMonthAnalysisForm,
        ShowDepositsForm,
        ShowCurrencyRatesForm,
        ShowOfficialRatesForm,
        ShowArticlesAssociationsForm,
        ShowBankDepositOffersForm,

        ShowDailyBalancesDiagram = 300,
        ShowRatesDiagram,
        ShowMonthlyResultDiagram,
        ShowMonthlyIncomeDiagram,
        ShowMonthlyOutcomeDiagram,
        ShowExpensePartingOxyPlotDiagram,
        ShowOxyplotRatesDiagram,
        ShowAverageSignificancesDiagram,

        ShowSettings = 400,
        TempItem,
        ShowToDoForm,
        ShowRegularPaymentsForm,

        ShowAboutForm = 500,
        QuitApplication,
    }
}