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
        }
    }
}
