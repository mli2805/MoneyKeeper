using System;
using System.Collections.Generic;
using System.Composition.Hosting;
using Caliburn.Micro;

using Keeper.Utils.MEF;

namespace Keeper
{
  public class AppBootstrapper : Bootstrapper<IShell>
  {
	  CompositionHost _compositionHost;

    protected override void Configure()
    {
		_compositionHost = new ContainerBuilder()
			.WithAssemblies(AssemblySource.Instance)
			.WithInstance<IWindowManager>(new WindowManager())
			.WithInstance<IEventAggregator>(new EventAggregator())
			.Build();

    }


	protected override object GetInstance(Type serviceType, string key)
	{
		return _compositionHost.GetExport(serviceType, key);
	}

	protected override IEnumerable<object> GetAllInstances(Type serviceType)
	{
		return _compositionHost.GetExports(serviceType);
	}

  }
}