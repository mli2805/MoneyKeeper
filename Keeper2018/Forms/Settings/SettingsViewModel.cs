using Caliburn.Micro;

namespace Keeper2018
{
    public class SettingsViewModel : Screen
    {
        public CardBalanceMemoViewModel CardBalanceMemoViewModel { get; set; }

        public SettingsViewModel(CardBalanceMemoViewModel cardBalanceMemoViewModel)
        {
            CardBalanceMemoViewModel = cardBalanceMemoViewModel;
        }

        public void Initialize()
        {
            CardBalanceMemoViewModel.Initialize();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Настрйки";
        }
    }
}
