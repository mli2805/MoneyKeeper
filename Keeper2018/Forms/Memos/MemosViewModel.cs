using System.Threading.Tasks;
using Caliburn.Micro;

namespace Keeper2018
{
    public class MemosViewModel : Screen
    {
        public CardBalanceMemoViewModel CardBalanceMemoViewModel { get; }
        public CardPaymentsLimitsViewModel CardPaymentsLimitsViewModel { get; }
        public DateMemoSetterViewModel DateMemoSetterViewModel { get; }

        public MemosViewModel(CardBalanceMemoViewModel cardBalanceMemoViewModel, 
            CardPaymentsLimitsViewModel cardPaymentsLimitsViewModel,
            DateMemoSetterViewModel dateMemoSetterViewModel)
        {
            CardBalanceMemoViewModel = cardBalanceMemoViewModel;
            CardPaymentsLimitsViewModel = cardPaymentsLimitsViewModel;
            DateMemoSetterViewModel = dateMemoSetterViewModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Напоминалки";
        }

        public async Task Initialize()
        {
            await CardBalanceMemoViewModel.Initialize();
            CardPaymentsLimitsViewModel.Initialize();
        }

    }
}
