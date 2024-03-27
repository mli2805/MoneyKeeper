using System.Diagnostics;
using System.IO;
using KeeperDomain;
using Serilog;

namespace Keeper2018
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using Autofac;
    using Caliburn.Micro;

    public class AppBootstrapper : BootstrapperBase
    {
        //        SimpleContainer container;
        private ILifetimeScope _container;

        public AppBootstrapper()
        {
            Initialize();
        }

        //        protected override void Configure() 
        //        {
        //            container = new SimpleContainer();
        //
        //            container.Singleton<IWindowManager, WindowManager>();
        //            container.Singleton<IEventAggregator, EventAggregator>();
        //            container.PerRequest<IShell, ShellViewModel>();
        //        }

        protected override object GetInstance(Type service, string key)
        {
            //  return container.GetInstance(service, key);
            return string.IsNullOrWhiteSpace(key) ?
                _container.Resolve(service) :
                _container.ResolveNamed(key, service);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            // return container.GetAllInstances(service);
            return _container.Resolve(typeof(IEnumerable<>).MakeGenericType(service)) as IEnumerable<object>;
        }

        protected override void BuildUp(object instance)
        {
            //  container.BuildUp(instance);
            _container.InjectProperties(instance);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<AutofacKeeper>();

            Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");

            _container = builder.Build();

            DisplayRootViewFor<IShell>();

            ConfigureLogger();
            Log.Information(Environment.NewLine + Environment.NewLine + new string('-', 80));
            Log.Information("AppBootstrapper::OnStartup");
        }

        private void ConfigureLogger()
        {
            var template = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(Path.Combine(PathFactory.GetLogsPath(), ".log"),
                    outputTemplate: template, rollingInterval: RollingInterval.Day,
                    flushToDiskInterval: TimeSpan.FromSeconds(1));

            if (Debugger.IsAttached)
            {
                loggerConfiguration.WriteTo.Console();
            }

            Log.Logger = loggerConfiguration.CreateLogger();
        }
    }
}