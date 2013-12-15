using System;
using System.Collections.Generic;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using Caliburn.Micro;

using Keeper.Utils.MEF;

namespace Keeper
{
  public class AppBootstrapper : Bootstrapper<IShell>
  {
	  CompositionHost mCompositionHost;

    protected override void Configure()
    {
		mCompositionHost = new ContainerBuilder()
			.WithAssemblies(AssemblySource.Instance)
			.WithInstance<IWindowManager>(new WindowManager())
			.WithInstance<IEventAggregator>(new EventAggregator())
			.Build();

    }


	protected override object GetInstance(Type serviceType, string key)
	{
		return mCompositionHost.GetExport(serviceType, key);
	}

	protected override IEnumerable<object> GetAllInstances(Type serviceType)
	{
		return mCompositionHost.GetExports(serviceType);
	}

  }
}