using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Keeper2018
{
    public class RatesViewModel : Screen
    {
        private readonly KeeperDataModel _keeperDataModel;
        private readonly IWindowManager _windowManager;
        public OfficialRatesViewModel OfficialRatesViewModel { get; }
        public ExchangeRatesViewModel ExchangeRatesViewModel { get; }
        public GoldRatesViewModel GoldRatesViewModel { get; }


        public RatesViewModel(OfficialRatesViewModel officialRatesViewModel,
            ExchangeRatesViewModel exchangeRatesViewModel, GoldRatesViewModel goldRatesViewModel,
            KeeperDataModel keeperDataModel, IWindowManager windowManager)
        {
            _keeperDataModel = keeperDataModel;
            _windowManager = windowManager;
            OfficialRatesViewModel = officialRatesViewModel;
            ExchangeRatesViewModel = exchangeRatesViewModel;
            GoldRatesViewModel = goldRatesViewModel;
        }

        public async Task Initialize()
        {
            await OfficialRatesViewModel.Initialize();
            ExchangeRatesViewModel.Initialize();
            GoldRatesViewModel.Initialize();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Курсы валют";
        }

        #region Charts

        private const string OxyplotKey = "A - Reset zoom  ;  Ctrl+RightMouse - Rectangle Zoom";
        public void LongTermChart()
        {
            var longTermChartViewModel = new LongTermChartViewModel();
            longTermChartViewModel.Initalize(OxyplotKey, OfficialRatesViewModel.Rows.ToList(), _keeperDataModel);
            _windowManager.ShowWindow(longTermChartViewModel);
        }

        public void UsdFourYearsChart()
        {
            var usdAnnualDiagramViewModel = new UsdAnnualDiagramViewModel();
            usdAnnualDiagramViewModel.Initialize(OxyplotKey, _keeperDataModel);
            _windowManager.ShowWindow(usdAnnualDiagramViewModel);
        }

        public void UsdFiveYearsChart()
        {
            var vm = new UsdFiveInOneChartViewModel();
            vm.Initialize(OxyplotKey, _keeperDataModel);
            _windowManager.ShowWindow(vm);
        }

        public void RusBelChart()
        {
            var vm = new RusBelChartViewModel();
            vm.Initialize(OxyplotKey, _keeperDataModel);
            _windowManager.ShowWindow(vm);
        }

        public void BasketChart()
        {
            var basketDiagramViewModel = new BasketDiagramViewModel();
            basketDiagramViewModel.Initalize(OxyplotKey, OfficialRatesViewModel.Rows.ToList());
            _windowManager.ShowWindow(basketDiagramViewModel);
        }

        public void ProbabilityChart()
        {
            var vm = new NbUsdProbabilitiesViewModel();
            vm.Initialize(OxyplotKey, OfficialRatesViewModel.Rows.ToList());
            _windowManager.ShowWindow(vm);
        }

        public void MonthlyChart()
        {
            var monthlyChartViewModel = new MonthlyChartViewModel();
            monthlyChartViewModel.Initialize(OxyplotKey, _keeperDataModel);
            _windowManager.ShowWindow(monthlyChartViewModel);
        }

        #endregion

        public void Close()
        {
            TryClose();
        }
    }
}
