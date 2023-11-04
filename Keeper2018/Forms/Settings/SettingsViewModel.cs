using Caliburn.Micro;

namespace Keeper2018
{
    public class SettingsViewModel : Screen
    {
        private readonly KeeperDataModel _keeperDataModel;
        public CardBalanceMemoViewModel CardBalanceMemoViewModel { get; set; }

        public SettingsViewModel(KeeperDataModel keeperDataModel, CardBalanceMemoViewModel cardBalanceMemoViewModel)
        {
            CardBalanceMemoViewModel = cardBalanceMemoViewModel;
            _keeperDataModel = keeperDataModel;
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
