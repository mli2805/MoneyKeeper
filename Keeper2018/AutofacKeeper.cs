using Autofac;
using Caliburn.Micro;

namespace Keeper2018
{
    public sealed class AutofacKeeper : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ShellViewModel>().As<IShell>();
            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();

            builder.RegisterType<OfficialRatesViewModel>().SingleInstance();
            builder.RegisterType<UsdAnnualDiagramViewModel>().SingleInstance();
            builder.RegisterType<BasketDiagramViewModel>().SingleInstance();

            builder.RegisterType<MainMenuViewModel>().SingleInstance();
            builder.RegisterType<AccountTreeViewModel>().SingleInstance();
            builder.RegisterType<AskDragAccountActionViewModel>().SingleInstance();
        }
    }
}
