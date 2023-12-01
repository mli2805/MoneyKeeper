using Caliburn.Micro;

namespace Keeper2018
{
    public class MemosViewModel : Screen
    {
        public CardBalanceMemoViewModel CardBalanceMemoViewModel { get; }
        public DateMemoSetterViewModel DateMemoSetterViewModel { get; }

        public MemosViewModel(CardBalanceMemoViewModel cardBalanceMemoViewModel, DateMemoSetterViewModel dateMemoSetterViewModel)
        {
            CardBalanceMemoViewModel = cardBalanceMemoViewModel;
            DateMemoSetterViewModel = dateMemoSetterViewModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Напоминалки";
        }

        public void Initialize()
        {
            CardBalanceMemoViewModel.Initialize();
        }

    }
}
