using Autofac;
using Caliburn.Micro;
using Keeper2018.PayCards;

namespace Keeper2018
{
    public sealed class AutofacKeeper : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();

            builder.RegisterType<KeeperDb>().SingleInstance();
            builder.RegisterType<DbLoader>().SingleInstance();
            builder.RegisterType<DbLoadingViewModel>().SingleInstance();

            builder.RegisterType<ShellPartsBinder>().SingleInstance();
            builder.RegisterType<ShellViewModel>().As<IShell>();

            builder.RegisterType<MainMenuViewModel>().SingleInstance();
            builder.RegisterType<BalanceOrTrafficViewModel>().SingleInstance();
            builder.RegisterType<TwoSelectorsViewModel>().SingleInstance();

            builder.RegisterType<AccountTreeViewModel>().SingleInstance();
            builder.RegisterType<AskDragAccountActionViewModel>().SingleInstance();
            builder.RegisterType<OneAccountViewModel>().SingleInstance();
            builder.RegisterType<OneDepositViewModel>().SingleInstance();
            builder.RegisterType<OneCardViewModel>().SingleInstance();
            builder.RegisterType<DepositReportViewModel>().SingleInstance();
            builder.RegisterType<BalanceVerificationViewModel>().SingleInstance();
            builder.RegisterType<PayCardsViewModel>().SingleInstance();

            builder.RegisterType<CurrencyRatesViewModel>().SingleInstance();
            builder.RegisterType<InputMyUsdViewModel>().SingleInstance();

            builder.RegisterType<MonthAnalysisViewModel>().SingleInstance();
            builder.RegisterType<MonthAnalyser>().SingleInstance();

            builder.RegisterType<BankOffersViewModel>().SingleInstance();
            builder.RegisterType<OneBankOfferViewModel>().SingleInstance();
            builder.RegisterType<RulesAndRatesViewModel>().SingleInstance();

            builder.RegisterType<ArticlesAssociationsViewModel>().SingleInstance();

            builder.RegisterType<ComboTreesProvider>().SingleInstance();
            builder.RegisterType<AssociationFinder>().SingleInstance();
            builder.RegisterType<AccNameSelectionControlInitializer>().SingleInstance();
            builder.RegisterType<BalanceDuringTransactionHinter>().SingleInstance();
            builder.RegisterType<UniversalControlVm>();
            builder.RegisterType<ReceiptViewModel>().SingleInstance();
            builder.RegisterType<OneTranViewModel>().SingleInstance();

            builder.RegisterType<FilterModel>().SingleInstance();
            builder.RegisterType<FilterViewModel>().SingleInstance();
            builder.RegisterType<AskReceiptDeletionViewModel>().SingleInstance();

            builder.RegisterType<TranEditExecutor>().SingleInstance();
            builder.RegisterType<TranMoveExecutor>().SingleInstance();
            builder.RegisterType<TranSelectExecutor>().SingleInstance();
            builder.RegisterType<TranModel>().SingleInstance();
            builder.RegisterType<TransactionsViewModel>().SingleInstance();

            builder.RegisterType<SettingsViewModel>().SingleInstance();
            builder.RegisterType<CarsViewModel>().SingleInstance();
            builder.RegisterType<GskViewModel>().SingleInstance();
            builder.RegisterType<CategoriesDataExtractor>().SingleInstance();
            builder.RegisterType<ExpenseByCategoriesViewModel>().SingleInstance();

        }
    }
}
