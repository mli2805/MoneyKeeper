using System.Collections.Generic;
using System.Composition;
using Caliburn.Micro;
using Keeper.Utils.DiagramDataExtraction;
using Keeper.Utils.DiagramDomainModel;
using Keeper.Utils.DiagramOxyPlots;
using Keeper.ViewModels.Diagram;

namespace Keeper.ViewModels.Shell.MainMenuActions
{
    [Export]
    public class MenuDiagramExecutor
    {
        private readonly BalancesDiagramsDataFactory _balancesDiagramsDataFactory;
        private readonly OldRatesDiagramDataFactory _oldRatesDiagramDataFactory;
        private readonly CategoriesDiagramsDataFactory _categoriesDiagramsDataFactory;
        private readonly CategoriesDataExtractor _categoriesDataExtractor;
        private readonly RatesOxyplotDataProvider _ratesOxyplotDataProvider;

        private readonly List<Screen> _launchedForms = new List<Screen>();

        private IWindowManager WindowManager { get; set; }
        [ImportingConstructor]
        public MenuDiagramExecutor(BalancesDiagramsDataFactory balancesDiagramsDataFactory, OldRatesDiagramDataFactory oldRatesDiagramDataFactory,
            CategoriesDiagramsDataFactory categoriesDiagramsDataFactory, CategoriesDataExtractor categoriesDataExtractor,
            RatesOxyplotDataProvider ratesOxyplotDataProvider)
        {
            _balancesDiagramsDataFactory = balancesDiagramsDataFactory;
            _oldRatesDiagramDataFactory = oldRatesDiagramDataFactory;
            _categoriesDiagramsDataFactory = categoriesDiagramsDataFactory;
            _categoriesDataExtractor = categoriesDataExtractor;
            _ratesOxyplotDataProvider = ratesOxyplotDataProvider;
            WindowManager = new WindowManager();
        }

        public bool Execute(MainMenuAction action)
        {
            switch (action)
            {
                case MainMenuAction.ShowDailyBalancesDiagram: ShowDailyBalancesDiagram(); break;
                case MainMenuAction.ShowRatesDiagram: ShowRatesDiagram(); break;
                case MainMenuAction.ShowMonthlyResultDiagram: ShowMonthlyResultDiagram(); break;
                case MainMenuAction.ShowMonthlyIncomeDiagram: ShowMonthlyIncomeDiagram(); break;
                case MainMenuAction.ShowMonthlyOutcomeDiagram: ShowMonthlyOutcomeDiagram(); break;
                case MainMenuAction.ShowExpensePartingOxyPlotDiagram: ShowExpensePartingOxyPlotDiagram(); break;
                case MainMenuAction.ShowOxyplotRatesDiagram: ShowOxyplotRatesDiagram(); break;
                case MainMenuAction.ShowAverageSignificancesDiagram: ShowAverageSignificancesDiagram(); break;
                default:
                    return false;
            }
            return false;
        }

        private void ShowDailyBalancesDiagram()
        {
            OpenDiagramForm(_balancesDiagramsDataFactory.DailyBalancesCtor());
        }
        private void ShowRatesDiagram()
        {
            OpenDiagramForm(_oldRatesDiagramDataFactory.RatesCtor());
        }
        private void ShowMonthlyResultDiagram()
        {
            OpenDiagramForm(_balancesDiagramsDataFactory.MonthlyResultsDiagramCtor());
        }
        private void ShowMonthlyIncomeDiagram()
        {
            OpenDiagramForm(_categoriesDiagramsDataFactory.MonthlyIncomesDiagramCtor());
        }
        private void ShowMonthlyOutcomeDiagram()
        {
            OpenDiagramForm(_categoriesDiagramsDataFactory.MonthlyExpenseDiagramCtor());
        }
        private void ShowExpensePartingOxyPlotDiagram()
        {
            var diagramData = _categoriesDataExtractor.GetGrouppedByMonth(false);
            var diagramOxyplotViewModel = new DiagramOxyplotViewModel(diagramData);
            WindowManager.ShowDialog(diagramOxyplotViewModel);
        }
        private void ShowOxyplotRatesDiagram()
        {
            var diagramData = _ratesOxyplotDataProvider.Get();
            var ratesOxyplotViewModel = new RatesOxyplotViewModel(diagramData);
            WindowManager.ShowDialog(ratesOxyplotViewModel);
        }
        private void ShowAverageSignificancesDiagram()
        {
            OpenDiagramForm(_categoriesDiagramsDataFactory.AverageOfMainCategoriesDiagramCtor());
        }
        private void OpenDiagramForm(DiagramData diagramData)
        {
            var diagramForm = new DiagramViewModel(diagramData);
            _launchedForms.Add(diagramForm);
            WindowManager.ShowWindow(diagramForm);
        }

    }
}
