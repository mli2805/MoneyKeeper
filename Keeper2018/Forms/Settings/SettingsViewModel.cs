using Caliburn.Micro;

namespace Keeper2018
{
    public class SettingsViewModel : Screen
    {
        public LargeExpenseThresholdViewModel LargeExpenseThresholdViewModel { get; set; }

        public SettingsViewModel(LargeExpenseThresholdViewModel largeExpenseThresholdViewModel)
        {
            LargeExpenseThresholdViewModel = largeExpenseThresholdViewModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Настрйки";
        }
    }
}
