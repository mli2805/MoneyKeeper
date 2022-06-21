﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class CurrencyRatesViewModel : Screen
    {
        private const string OxyplotKey = "A - Reset zoom  ;  Ctrl+RightMouse - Rectangle Zoom";

        private readonly IWindowManager _windowManager;
        private readonly KeeperDataModel _keeperDataModel;
        private readonly InputMyUsdViewModel _inputMyUsdViewModel;
        private readonly GoldCoinsViewModel _goldCoinsViewModel;

        private Dictionary<DateTime, CurrencyRates> _rates;
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

        public CurrencyRatesViewModel(IWindowManager windowManager, KeeperDataModel keeperDataModel,
            InputMyUsdViewModel inputMyUsdViewModel, GoldCoinsViewModel goldCoinsViewModel)
        {
            _windowManager = windowManager;
            _keeperDataModel = keeperDataModel;
            _inputMyUsdViewModel = inputMyUsdViewModel;
            _goldCoinsViewModel = goldCoinsViewModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Currency rates";
            SelectedRow = Rows?.LastOrDefault();
        }

        public void Initialize()
        {
            _rates = _keeperDataModel.Rates;
            Task.Factory.StartNew(Init);
            IsDownloadEnabled = true;
        }

        private void Init()
        {
            CurrencyRatesModel annual = null;
            CurrencyRatesModel previous = null;
            foreach (var record in _rates)
            {
                var current = new CurrencyRatesModel(record.Value, previous, annual);
                if (Application.Current.Dispatcher != null)
                    Application.Current.Dispatcher.Invoke(() => Rows.Add(current));

                if (!current.BasketDelta.Equals(0))
                    previous = current;
                if (current.Date.Day == 31 && current.Date.Month == 12)
                    annual = current;
            }

            if (Application.Current.Dispatcher != null)
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
            usdAnnualDiagramViewModel.Initialize(OxyplotKey, Rows.ToList());
            _windowManager.ShowWindow(usdAnnualDiagramViewModel);
        }

        public void UsdFiveYearsChart()
        {
            var vm = new UsdFiveInOneChartViewModel();
            vm.Initialize(OxyplotKey, Rows.ToList());
            _windowManager.ShowWindow(vm);
        }

        public void RusBelChart()
        {
            var vm = new RusBelChartViewModel();
            vm.Initialize(OxyplotKey, Rows.ToList());
            _windowManager.ShowWindow(vm);
        }

        public void ProbabilityChart()
        {
            var vm = new NbUsdProbabilitiesViewModel();
            vm.Initialize(OxyplotKey, Rows.ToList());
            _windowManager.ShowWindow(vm);
        }

        public void BasketChart()
        {
            var basketDiagramViewModel = new BasketDiagramViewModel();
            basketDiagramViewModel.Initalize(OxyplotKey, Rows.ToList());
            _windowManager.ShowWindow(basketDiagramViewModel);
        }

        public void MonthlyChart()
        {
            var monthlyChartViewModel = new MonthlyChartViewModel();
            monthlyChartViewModel.Initialize(OxyplotKey, Rows.ToList());
            _windowManager.ShowWindow(monthlyChartViewModel);
        }

        public void GoldView()
        {
            _goldCoinsViewModel.Initialize();
            _windowManager.ShowWindow(_goldCoinsViewModel);
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
                    var currencyRates = await DownloadRates(date);
                    if (currencyRates == null) break;

                    currencyRates.Id = Rows.Last().Id + 1;
                    _rates.Add(currencyRates.Date, currencyRates);
                    var line = new CurrencyRatesModel(currencyRates, Rows.Last(), annual);
                    Rows.Add(line);

                    if (date.Date.Day == 31 && date.Date.Month == 12)
                        annual = line;
                    date = date.AddDays(1);
                }
            }
            IsDownloadEnabled = true;
        }


        public async void DlMonth()
        {
            var result = new List<string>();
            using (new WaitCursor())
            {
                var date = new DateTime(2022, 5, 28);
                double prevRate = 0;
                while (date <= DateTime.Today)
                {
                    var usd2Rur = await CbrRatesDownloader.GetRateForDateFromXml(date);
                    if (usd2Rur == 0)
                        usd2Rur = prevRate;
                    else
                        prevRate = usd2Rur;

                    var dd = _rates[date];
                    dd.CbrRate.Usd.Value = usd2Rur;

                    result.Add($"{date:D} - {usd2Rur:##0.####}");
                    date = date.AddDays(1);
                }

            }
            File.WriteAllLines(@"c:\temp\cbrf.txt", result);
        }

        private async Task<CurrencyRates> DownloadRates(DateTime date)
        {
            var nbRbRates = await NbRbRatesDownloader.GetRatesForDate(date);
            if (nbRbRates == null) return null;
            var currencyRates = new CurrencyRates() { Date = date, NbRates = nbRbRates };
            var usd2Rur = await CbrRatesDownloader.GetRateForDateFromXml(date);
            currencyRates.CbrRate.Usd = new OneRate() { Unit = 1, Value = usd2Rur };
            return currencyRates;
        }

        public void Input()
        {
            var previousLine = _rates[SelectedRow.Date.AddDays(-1)];
            _inputMyUsdViewModel.Initialize(SelectedRow, previousLine);
            _windowManager.ShowDialog(_inputMyUsdViewModel);
        }

        public void RemoveLine()
        {
            _rates.Remove(SelectedRow.Date);
            Rows.Remove(SelectedRow);
        }

        public void Close()
        {
            TryClose();
        }

    }
}
