using System.Collections.Generic;
using System.Composition;
using Caliburn.Micro;
using Keeper.DomainModel.DbTypes;
using Keeper.ViewModels.Deposits;
using Keeper.ViewModels.SingleViews;
using Keeper.ViewModels.TransWithTags;

namespace Keeper.ViewModels.Shell.MainMenuActions
{
    [Export]
    public class MenuFormsExecutor
    {
        private readonly KeeperDb _db;
        private IWindowManager WindowManager { get; set; }
        private readonly List<Screen> _launchedForms = new List<Screen>();

        [ImportingConstructor]
        public MenuFormsExecutor(KeeperDb db)
        {
            _db = db;
            WindowManager = new WindowManager();
        }

        public bool Execute(MainMenuAction action)
        {
            switch (action)
            {
                case MainMenuAction.ShowTransactionsForm: return ShowTransactionsForm();
                case MainMenuAction.ShowCurrencyRatesForm: return ShowCurrencyRatesForm();
                case MainMenuAction.ShowOfficialRatesForm: return ShowOfficialRatesForm();
                case MainMenuAction.ShowArticlesAssociationsForm: return ShowArticlesAssociationsForm();
                case MainMenuAction.ShowMonthAnalysisForm: return ShowMonthAnalysisForm();
                case MainMenuAction.ShowDepositsForm: return ShowDepositsForm();
                case MainMenuAction.ShowBankDepositOffersForm: return ShowBankDepositOffersForm();
            }
            return false;
        }

        public bool ShowTransactionsForm()
        {
            var trForm = new TransViewModel(_db);
            WindowManager.ShowDialog(trForm);
            return trForm.IsCollectionChanged;
        }

        public bool ShowCurrencyRatesForm()
        {
            var ratesViewModel = IoC.Get<RatesViewModel>();
            WindowManager.ShowDialog(ratesViewModel);
            return ratesViewModel.IsCollectionChanged;
        }

        public bool ShowOfficialRatesForm()
        {
            var nbRatesViewModel = IoC.Get<NbRatesViewModel>();
            WindowManager.ShowDialog(nbRatesViewModel);
            return nbRatesViewModel.IsCollectionChanged;
        }

        public bool ShowArticlesAssociationsForm()
        {
            WindowManager.ShowDialog(IoC.Get<ArticlesAssociationsViewModel>());
            return true;
        }

        public bool ShowMonthAnalysisForm()
        {
            WindowManager.ShowDialog(IoC.Get<MonthAnalysisViewModel>());
            return false;
        }

        public bool ShowDepositsForm()
        {
//            foreach (var launchedForm in LaunchedForms)
//                if (launchedForm is DepositViewModel && launchedForm.IsActive) launchedForm.TryClose();

            var depositsForm = IoC.Get<DepositsViewModel>();

            _launchedForms.Add(depositsForm);
            WindowManager.ShowWindow(depositsForm);
            return false;
        }

        public bool ShowBankDepositOffersForm()
        {
            WindowManager.ShowDialog(IoC.Get<BankDepositOffersViewModel>());
            return true;
        }

    }
}