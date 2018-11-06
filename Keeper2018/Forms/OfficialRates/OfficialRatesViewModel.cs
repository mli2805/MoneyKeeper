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
    public class OfficialRatesViewModel : Screen
    {
        private readonly IWindowManager _windowManager;
        private readonly UsdAnnualDiagramViewModel _usdAnnualDiagramViewModel;
        private readonly BasketDiagramViewModel _basketDiagramViewModel;

        private List<OfficialRates> _rates;
        public ObservableCollection<OfficialRatesModel> Rows { get; set; }
        public OfficialRatesModel SelectedRow { get; set; }

        public OfficialRatesModel LastDayOfYear { get; set; }

        private bool _isDownloadEnabled;
        public bool IsDownloadEnabled
        {
            get { return _isDownloadEnabled; }
            set
            {
                if (value == _isDownloadEnabled) return;
                _isDownloadEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public OfficialRatesViewModel(IWindowManager windowManager, KeeperDb keeperDb,
            UsdAnnualDiagramViewModel usdAnnualDiagramViewModel,
            BasketDiagramViewModel basketDiagramViewModel)
        {
            _windowManager = windowManager;
            _rates = keeperDb.OfficialRates;
            _usdAnnualDiagramViewModel = usdAnnualDiagramViewModel;
            _basketDiagramViewModel = basketDiagramViewModel;
            Task.Factory.StartNew(Init);
           IsDownloadEnabled = true;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Official rates";
            SelectedRow = Rows?.LastOrDefault();
        }

        private void Init()
        {
            Rows = new ObservableCollection<OfficialRatesModel>();
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
