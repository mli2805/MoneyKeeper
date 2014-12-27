using System.Collections.Generic;
using System.Composition.Hosting.Core;

namespace Keeper.Utils.MEF
{
	public class InstanceExportDescriptor<T> : ExportDescriptor
	{
		readonly T mInstance;

		public InstanceExportDescriptor(T instance)
		{
			mInstance = instance;
		}

		public override CompositeActivator Activator
		{
			get { return (l, op) => mInstance; }
		}
		public override IDictionary<string, object> Metadata
		{
			get { return null; }
		}
	}
}