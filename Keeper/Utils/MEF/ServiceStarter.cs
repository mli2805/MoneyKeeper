using System.Composition.Hosting;

namespace Keeper.Utils.MEF
{
    public static class ServiceStarter
    {
        public static void StartServices(CompositionHost compositionHost)
        {
            foreach (var component in compositionHost.GetExports<IStartUp>()) component.Start();
        }
    }
	 
}