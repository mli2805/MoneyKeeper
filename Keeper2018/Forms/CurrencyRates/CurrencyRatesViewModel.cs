using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

namespace Keeper2018
{
    public class CurrencyRatesViewModel : Screen
    {
        private const string OxyplotKey = "A - Reset zoom  ;  Ctrl+RightMouse - Rectangle Zoom";

        private readonly IWindowManager _windowManager;
        private readonly KeeperDb _keeperDb;
        private readonly InputMyUsdViewModel _inputMyUsdViewModel;

        private List<CurrencyRates> _rates;
        public ObservableCollection<CurrencyRatesModel> Rows { get; set; } = new ObservableCollection<CurrencyRatesModel>();

        public CurrencyRatesModel SelectedRow
        {
            get => _selectedRow;
            set
            {
                if (Equals(value, _selectedRow)) return;
                _selectedRow = value;
                NotifyOfPropertyChange();
            }
        }

        public CurrencyRatesModel LastDayOfYear { get; set; }

        private bool _isDownloadEnabled;
        private CurrencyRatesModel _selectedRow;

        public bool IsDownloadEnabled
        {
            get => _isDownloadEnabled;
            set
            {
                if (value == _isDownloadEnabled) return;
                _isDownloadEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public CurrencyRatesViewModel(IWindowManager windowManager, KeeperDb keeperDb,
            InputMyUsdViewModel inputMyUsdViewModel)
        {
            _windowManager = windowManager;
            _keeperDb = keeperDb;
            _inputMyUsdViewModel = inputMyUsdViewModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Currency rates";
            SelectedRow = Rows?.LastOrDefault();
        }

        public void Initialize()
        {
            _rates = _keeperDb.Bin.OfficialRates;
            Task.Factory.StartNew(Init);
            IsDownloadEnabled = true;
        }

        private void Init()
        {
            CurrencyRatesModel annual = null;
            CurrencyRatesModel previous = null;
            foreach (var record in _rates)
            {
                var current = new CurrencyRatesModel(record, previous, annual);
                Application.Current.Dispatcher.Invoke(() => Rows.Add(current));

                if (!current.BasketDelta.Equals(0))
                    previous = current;
                if (current.Date.Day == 31 && current.Date.Month == 12)
                    annual = current;
            }
            Application.Current.Dispatcher.Invoke(() => LastDayOfYear = annual);
        }

        public void LongTermChart()
        {
            var longTermChartViewModel = new LongTermChartViewModel();
            longTermChartViewModel.Initalize(OxyplotKey, Rows.ToList());
            _windowManager.ShowWindow(longTermChartViewModel);
        }

        public void UsdFourYearsChart()
        {
            var usdAnnualDiagramViewModel = new UsdAnnualDiagramViewModel();
            usdAnnualDiagramViewModel.Initalize(OxyplotKey, Rows.ToList());
            _windowManager.ShowWindow(usdAnnualDiagramViewModel);
        }

        public void BasketChart()
        {
            var basketDiagramViewModel = new BasketDiagramViewModel();
            basketDiagramViewModel.Initalize(OxyplotKey, Rows.ToList());
            _windowManager.ShowWindow(basketDiagramViewModel);
        }

        public async void Download()
        {
            IsDownloadEnabled = false;
            using (new WaitCursor())
            {
                var date = Rows.Last().Date.AddDays(1);
                var annual = Rows.Last(r => r.Date.Day == 31 && r.Date.Month == 12);
                while (date <= DateTime.Today.Date.AddDays(1))
                {
                    var nbRbRates = await NbRbRatesDownloader.GetRatesForDate(date);
                    if (nbRbRates == null) break;
                    var currencyRates = new CurrencyRates() { Date = date, NbRates = nbRbRates };
                    var usd2Rur = await CbrRatesDownloader.GetRateForDate(date);
                    currencyRates.CbrRate.Usd = new OneRate() { Unit = 1, Value = usd2Rur };
                    currencyRates.MyUsdRate = new OneRate(){Value = nbRbRates.Usd.Value * 1.003, Unit = 1};

                    _rates.Add(currencyRates);
                    var line = new CurrencyRatesModel(currencyRates, Rows.Last(), annual);
                    Rows.Add(line);

                    if (date.Date.Day == 31 && date.Date.Month == 12)
                        annual = line;
                    date = date.AddDays(1);
                }
            }
            IsDownloadEnabled = true;
        }

        public void InputMyUsd()
        {
            _inputMyUsdViewModel.CurrencyRatesModel = SelectedRow;
            var previousLine = _rates.FirstOrDefault(r => r.Date.Equals(SelectedRow.Date.AddDays(-1)));
            if (previousLine != null)
                _inputMyUsdViewModel.MyUsdRate = previousLine.MyUsdRate.Value;
            _windowManager.ShowDialog(_inputMyUsdViewModel);
            if (_inputMyUsdViewModel.IsSavePressed)
            {
                var rateLine = _rates.First(r => r.Date.Equals(SelectedRow.Date));
                rateLine.MyUsdRate.Value = _inputMyUsdViewModel.MyUsdRate;
                SelectedRow.InputMyUsd(_inputMyUsdViewModel.MyUsdRate);
            }
        }

        public void RemoveLine()
        {
            var rateLine = _rates.First(r => r.Date.Equals(SelectedRow.Date));
            _rates.Remove(rateLine);
            Rows.Remove(SelectedRow);
        }

        public async void CbrDownload()
        {
            IsDownloadEnabled = false;
            using (new WaitCursor())
            {
                foreach (var model in Rows)
                {
                    if (string.IsNullOrEmpty(model.RurUsdStr))
                    {
                        double usd2Rur;
                        try
                        {
                            usd2Rur = await CbrRatesDownloader.GetRateForDate(model.Date);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Error: " + e.Message);
                            break;
                        }
                        var rate = _rates.First(r => r.Date == model.Date);
                        rate.CbrRate.Usd = new OneRate() { Unit = 1, Value = usd2Rur };
                        model.RurUsdStr = usd2Rur.ToString("#,#.##", new CultureInfo("ru-RU"));
                    }
                }
            }
            IsDownloadEnabled = true;
        }

        public void Close()
        {

            TryClose();
        }

    }
}
