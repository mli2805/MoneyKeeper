using System;
using System.Collections.Generic;
using System.Composition.Hosting;
using System.Composition.Hosting.Core;
using System.Reflection;

namespace Keeper.Utils.MEF
{
	public class ContainerBuilder
	{
		readonly List<Assembly> mAssemblies = new List<Assembly>();
		readonly List<ExportDescriptorProvider> mMocks = new List<ExportDescriptorProvider>();
		readonly HashSet<Type> mExcludes = new HashSet<Type>();

		public ContainerBuilder WithAssembly(Assembly assembly)
		{
			mAssemblies.Add(assembly);
			return this;
		}
		public ContainerBuilder WithAssemblies(IEnumerable<Assembly> assemblies)
		{
			mAssemblies.AddRange(assemblies);
			return this;
		}
		public sealed class ReplaceImpl
		{
			readonly ContainerBuilder mBuilder;

			public ReplaceImpl(ContainerBuilder builder)
			{
				mBuilder = builder;
			}

			public ContainerBuilder WithMock<T>(T mock)
			{
				mBuilder.mMocks.Add(new InstanceDescriptorProvider<T>(mock));
				return mBuilder;
			}
		}
		public ReplaceImpl Replace<TReplaced>()
		{
			mExcludes.Add(typeof(TReplaced));
			return new ReplaceImpl(this);
		}
		public ContainerBuilder WithMock<T>(T mock)
		{
			mExcludes.Add(typeof(T));
			mMocks.Add(new InstanceDescriptorProvider<T>(mock));
			return this;
		}
		public ContainerBuilder WithInstance<T>(T mock)
		{
			mMocks.Add(new InstanceDescriptorProvider<T>(mock));
			return this;
		}

		public CompositionHost Build()
		{
			var containerConfiguration = new ContainerConfiguration();
			foreach (var assembly in mAssemblies) containerConfiguration.WithAssembly(assembly, mExcludes);
			foreach (var mock in mMocks) containerConfiguration.WithProvider(mock);
			return containerConfiguration.CreateContainer();
		}

		public ContainerBuilder ReplaceInstance<T>(T instance)
		{
			mMocks.RemoveAll(d => d as InstanceDescriptorProvider<T> != null);
			mMocks.Add(new InstanceDescriptorProvider<T>(instance));
			return this;
		}
	}
}