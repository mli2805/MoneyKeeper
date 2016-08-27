using System.Collections.Generic;

namespace Keeper.ViewModels.Shell.MainMenuActions
{
    public enum MainMenuAction 
    {
        DoNothing = 0,

        SaveDatabase = 100,
        LoadDatabase,
        ClearDatabase,
        MakeDatabaseBackup,
        ExportDatabaseToTxt,
        ImportDatabaseFromTxt,
        RemoveExtraBackups,
        RemoveAllNonFirstInMonth,

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

    public class MainMenuActionProperties
    {
        public bool IsAsync { get; set; }
        public string StatusBarMessage { get; set; }
    }

    public class MainMenu
    {
        public Dictionary<MainMenuAction, MainMenuActionProperties> Actions { get; set; }

        public void Init()
        {
            Actions = new Dictionary<MainMenuAction, MainMenuActionProperties>
            {
                {MainMenuAction.DoNothing, new MainMenuActionProperties {IsAsync = false, StatusBarMessage = "Готов"}},

                {MainMenuAction.SaveDatabase,          new MainMenuActionProperties {IsAsync = true, StatusBarMessage = "Сохранение данных на диск..."}},
                {MainMenuAction.LoadDatabase,          new MainMenuActionProperties {IsAsync = true, StatusBarMessage = "Загрузка данных с диска..."}},
                {MainMenuAction.ClearDatabase,         new MainMenuActionProperties {IsAsync = true, StatusBarMessage = "Очистка БД..."}},
                {MainMenuAction.MakeDatabaseBackup,    new MainMenuActionProperties {IsAsync = true, StatusBarMessage = "..."}},
                {MainMenuAction.ExportDatabaseToTxt,   new MainMenuActionProperties {IsAsync = true, StatusBarMessage = "..."}},
                {MainMenuAction.ImportDatabaseFromTxt, new MainMenuActionProperties {IsAsync = true, StatusBarMessage = "..."}},
                {MainMenuAction.RemoveExtraBackups,    new MainMenuActionProperties {IsAsync = true, StatusBarMessage = "Удаление идентичных резервных копий..."}},
                {MainMenuAction.RemoveAllNonFirstInMonth, new MainMenuActionProperties {IsAsync = true, StatusBarMessage = "Удаление всех не первых в месяце архивов..."}},

                {MainMenuAction.ShowTransactionsForm,         new MainMenuActionProperties {IsAsync = false, StatusBarMessage = "Ввод транзакций"}},
                {MainMenuAction.ShowMonthAnalysisForm,        new MainMenuActionProperties {IsAsync = false, StatusBarMessage = "Анализ месяца"}},
                {MainMenuAction.ShowDepositsForm,             new MainMenuActionProperties {IsAsync = false, StatusBarMessage = "..."}},
                {MainMenuAction.ShowCurrencyRatesForm,        new MainMenuActionProperties {IsAsync = false, StatusBarMessage = "Ввод курсов валют"}},
                {MainMenuAction.ShowOfficialRatesForm,        new MainMenuActionProperties {IsAsync = false, StatusBarMessage = "Курсы НБ РБ"}},
                {MainMenuAction.ShowArticlesAssociationsForm, new MainMenuActionProperties {IsAsync = false, StatusBarMessage = "Ввод ассоциаций"}},
                {MainMenuAction.ShowBankDepositOffersForm,    new MainMenuActionProperties {IsAsync = false, StatusBarMessage = "..."}},

                {MainMenuAction.ShowDailyBalancesDiagram,         new MainMenuActionProperties {IsAsync = false, StatusBarMessage = "Диаграммы..."}},
                {MainMenuAction.ShowRatesDiagram,                 new MainMenuActionProperties {IsAsync = false, StatusBarMessage = "Диаграммы..."}},
                {MainMenuAction.ShowMonthlyResultDiagram,         new MainMenuActionProperties {IsAsync = false, StatusBarMessage = "Диаграммы..."}},
                {MainMenuAction.ShowMonthlyIncomeDiagram,         new MainMenuActionProperties {IsAsync = false, StatusBarMessage = "Диаграммы..."}},
                {MainMenuAction.ShowMonthlyOutcomeDiagram,        new MainMenuActionProperties {IsAsync = false, StatusBarMessage = "Диаграммы..."}},
                {MainMenuAction.ShowExpensePartingOxyPlotDiagram, new MainMenuActionProperties {IsAsync = false, StatusBarMessage = "Диаграммы..."}},
                {MainMenuAction.ShowOxyplotRatesDiagram,          new MainMenuActionProperties {IsAsync = false, StatusBarMessage = "Диаграммы..."}},
                {MainMenuAction.ShowAverageSignificancesDiagram,  new MainMenuActionProperties {IsAsync = false, StatusBarMessage = "Диаграммы..."}},

                {MainMenuAction.ShowSettings,            new MainMenuActionProperties {IsAsync = false, StatusBarMessage = "Ввод констант"}},
                {MainMenuAction.TempItem,                new MainMenuActionProperties {IsAsync = false, StatusBarMessage = "..."}},
                {MainMenuAction.ShowToDoForm,            new MainMenuActionProperties {IsAsync = false, StatusBarMessage = "Ввод TO DO"}},
                {MainMenuAction.ShowRegularPaymentsForm, new MainMenuActionProperties {IsAsync = false, StatusBarMessage = "Ввод регулярных платежей"}},

                {MainMenuAction.ShowAboutForm,   new MainMenuActionProperties {IsAsync = false, StatusBarMessage = "About form"}},
                {MainMenuAction.QuitApplication, new MainMenuActionProperties {IsAsync = true, StatusBarMessage = "Идет завершение программы..."}},
            };
        }
    }
}