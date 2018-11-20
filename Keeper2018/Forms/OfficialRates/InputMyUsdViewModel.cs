using Caliburn.Micro;

namespace Keeper2018
{
    public class InputMyUsdViewModel : Screen
    {
        public OfficialRatesModel OfficialRatesModel { get;set; }

        public double MyUsdRate { get; set; }

        public bool IsSavePressed;

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Ввод курса";
        }

        public void Save()
        {
            IsSavePressed = true;
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}
