﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

namespace Keeper2018
{
    public class OfficialRatesViewModel : Screen
    {
        private readonly IWindowManager _windowManager;
        private readonly KeeperDb _keeperDb;
        private readonly InputMyUsdViewModel _inputMyUsdViewModel;
        private readonly UsdAnnualDiagramViewModel _usdAnnualDiagramViewModel;
        private readonly BasketDiagramViewModel _basketDiagramViewModel;

        private List<OfficialRates> _rates;
        public ObservableCollection<OfficialRatesModel> Rows { get; set; } = new ObservableCollection<OfficialRatesModel>();

        public OfficialRatesModel SelectedRow
        {
            get => _selectedRow;
            set
            {
                if (Equals(value, _selectedRow)) return;
                _selectedRow = value;
                NotifyOfPropertyChange();
            }
        }

        public OfficialRatesModel LastDayOfYear { get; set; }

        private bool _isDownloadEnabled;
        private OfficialRatesModel _selectedRow;

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

        public OfficialRatesViewModel(IWindowManager windowManager, KeeperDb keeperDb,
            InputMyUsdViewModel inputMyUsdViewModel,
            UsdAnnualDiagramViewModel usdAnnualDiagramViewModel,
            BasketDiagramViewModel basketDiagramViewModel)
        {
            _windowManager = windowManager;
            _keeperDb = keeperDb;
            _inputMyUsdViewModel = inputMyUsdViewModel;
            _usdAnnualDiagramViewModel = usdAnnualDiagramViewModel;
            _basketDiagramViewModel = basketDiagramViewModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Official rates";
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
            OfficialRatesModel annual = null;
            OfficialRatesModel previous = null;
            foreach (var record in _rates)
            {
                var current = new OfficialRatesModel(record, previous, annual);
                Application.Current.Dispatcher.Invoke(() => Rows.Add(current));

                if (!current.BasketDelta.Equals(0))
                    previous = current;
                if (current.Date.Day == 31 && current.Date.Month == 12)
                    annual = current;
            }
            Application.Current.Dispatcher.Invoke(() => LastDayOfYear = annual);
        }

        public void UsdChart()
        {
            _usdAnnualDiagramViewModel.Initalize(Rows.ToList());
            _windowManager.ShowDialog(_usdAnnualDiagramViewModel);
        }

        public void BasketChart()
        {
            _basketDiagramViewModel.Initalize(Rows.ToList());
            _windowManager.ShowDialog(_basketDiagramViewModel);
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
                    var officialRates = new OfficialRates() { Date = date, NbRates = nbRbRates };
                    var usd2Rur = await CbrRatesDownloader.GetRateForDate(date);
                    officialRates.CbrRate.Usd = new OneRate() { Unit = 1, Value = usd2Rur };

                    _rates.Add(officialRates);
                    var line = new OfficialRatesModel(officialRates, Rows.Last(), annual);
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
            _inputMyUsdViewModel.OfficialRatesModel = SelectedRow;
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
