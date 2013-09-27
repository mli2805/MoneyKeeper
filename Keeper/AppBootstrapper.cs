using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using Caliburn.Micro;

namespace Keeper
{
  public class AppBootstrapper : Bootstrapper<IShell>
  {
    private CompositionContainer _container;

    protected override void Configure()
    {
      _container = new CompositionContainer(new AggregateCatalog(
        AssemblySource.Instance.Select(x => new AssemblyCatalog(x))));

      var batch = new CompositionBatch();

      batch.AddExportedValue<IWindowManager>(new WindowManager());
      batch.AddExportedValue<IEventAggregator>(new EventAggregator());
      batch.AddExportedValue(_container);
      
      _container.Compose(batch);
    }

    protected override object GetInstance(Type serviceType, string key)
    {
      var contract = string.IsNullOrEmpty(key) 
        ? AttributedModelServices.GetContractName(serviceType) 
        : key;

      var exports = _container.GetExportedValues<object>(contract);

      var first = exports.FirstOrDefault();
      if (first != null) return first;

      throw new Exception(string.Format(
        "Could not locate any instances of contract {0}.", contract));
    }

    protected override IEnumerable<object> GetAllInstances(Type serviceType)
    {
      return _container.GetExportedValues<object>(
        AttributedModelServices.GetContractName(serviceType));
    }

    protected override void BuildUp(object instance)
    {
      _container.SatisfyImportsOnce(instance);
    }

    protected override IEnumerable<Assembly> SelectAssemblies()
    {
      yield return typeof(AppBootstrapper).Assembly;
    }
  }
}