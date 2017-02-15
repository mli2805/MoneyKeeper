using System.Collections.Generic;
using System.Composition.Hosting.Core;

namespace Keeper.Utils.MEF
{
	public class InstanceDescriptorProvider<T> : ExportDescriptorProvider
	{
		readonly object mInstance;

		public InstanceDescriptorProvider(object instance)
		{
			mInstance = instance;
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors(
			CompositionContract contract, DependencyAccessor descriptorAccessor)
		{
			if (contract.ContractType == typeof(T))
				return new[] { 
					new ExportDescriptorPromise(
						contract, "Manual export", 
						false, NoDependencies, 
						d => new InstanceExportDescriptor<T>((T)mInstance)) };
			//Console.WriteLine("Manual export: "+contract);
			return NoExportDescriptors;
		}
	}
}