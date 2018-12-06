using Autofac;
using Caliburn.Micro;

namespace Keeper2018
{
    public sealed class AutofacKeeper : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();

            builder.RegisterType<KeeperDb>().SingleInstance();
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
            builder.RegisterType<DepositReportViewModel>().SingleInstance();

            builder.RegisterType<OfficialRatesViewModel>().SingleInstance();
            builder.RegisterType<InputMyUsdViewModel>().SingleInstance();
            builder.RegisterType<UsdAnnualDiagramViewModel>().SingleInstance();
            builder.RegisterType<BasketDiagramViewModel>().SingleInstance();

            builder.RegisterType<MonthAnalysisViewModel>().SingleInstance();

            builder.RegisterType<BankOffersViewModel>().SingleInstance();
            builder.RegisterType<OneBankOfferViewModel>().SingleInstance();
            builder.RegisterType<RulesAndRatesViewModel>().SingleInstance();

            builder.RegisterType<ArticlesAssociationsViewModel>().SingleInstance();
            builder.RegisterType<TransactionsViewModel>().SingleInstance();
        }
    }
}
