using Autofac;
using Caliburn.Micro;

namespace Keeper2018
{
    public class MainMenuViewModel : PropertyChangedBase
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;

        public MainMenuViewModel(ILifetimeScope globalScope, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
        }

        public void ShowOfficialRates()
        {
            var vm = _globalScope.Resolve<OfficialRatesViewModel>();
            _windowManager.ShowDialog(vm);
        }
    }
}
