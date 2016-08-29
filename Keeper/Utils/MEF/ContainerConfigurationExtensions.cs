using System;
using System.Collections.Generic;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;

namespace Keeper.Utils.MEF
{
	public static class ContainerConfigurationExtensions
	{
		public static ContainerConfiguration WithInstance<T>(this ContainerConfiguration configuration, T instance)
		{
			configuration.WithProvider(new InstanceDescriptorProvider<T>(instance));
			return configuration;
		}

		public static ContainerConfiguration WithAssembly(this ContainerConfiguration configuration, Assembly assembly, HashSet<Type> except)
		{
			return configuration.WithParts(assembly.DefinedTypes.Select(dt => dt.AsType()).Where(t => !except.Contains(t)));
		}

		//public static ContainerConfiguration WithMock<T>(this ContainerConfiguration configuration, T mock)
		//{
		//	configuration = configuration.WithAssembly(configuration.Assemblies, except:...);
		//	configuration.WithProvider(new InstanceDescriptorProvider<T>(mock));
		//	return configuration;
		//}
	}
}