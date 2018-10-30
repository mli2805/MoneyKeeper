using System;
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
        public ObservableCollection<NbRbRateOnScreen> Rows { get; set; }
        public NbRbRateOnScreen SelectedRow { get; set; }

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

        public OfficialRatesViewModel(IWindowManager windowManager, UsdAnnualDiagramViewModel usdAnnualDiagramViewModel,
            BasketDiagramViewModel basketDiagramViewModel)
        {
            _windowManager = windowManager;
            _usdAnnualDiagramViewModel = usdAnnualDiagramViewModel;
            _basketDiagramViewModel = basketDiagramViewModel;
            Task.Factory.StartNew(Init);
            IsDownloadEnabled = true;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "NB RB. Official rates.";
            SelectedRow = Rows.LastOrDefault();
        }

        private void Init()
        {
            if (Rows != null) return;
            Rows = new ObservableCollection<NbRbRateOnScreen>();
            NbRbRateOnScreen annual = null;
            NbRbRateOnScreen previous = null;
            foreach (var record in NbRbRatesOldTxt.LoadFromOldTxt())
            {
                var current = new NbRbRateOnScreen(record, previous, annual);
                Application.Current.Dispatcher.Invoke(() => Rows.Add(current));
                if (!current.BasketDelta.Equals(0))
                    previous = current;
                if (current.Date.Day == 31 && current.Date.Month == 12)
                    annual = current;
            }
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
                    var ratesFromSite = await NbRbRatesDownloader.GetRatesForDate(date);
                    if (ratesFromSite == null) break;
                    var nbRbRate = new NbRbRate() { Date = date, Values = ratesFromSite };
                    var line = new NbRbRateOnScreen(nbRbRate, Rows.Last(), annual);

                    var usd2Rur = await CbrRatesDownloader.GetRateForDate(date);
                    line.RurUsdStr = usd2Rur.ToString("#,#.##", new CultureInfo("ru-RU"));
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
                foreach (var nbRbRateOnScreen in Rows)
                {
                    if (string.IsNullOrEmpty(nbRbRateOnScreen.RurUsdStr))
                    {
                        var usd2Rur = await CbrRatesDownloader.GetRateForDate(nbRbRateOnScreen.Date);
                        nbRbRateOnScreen.RurUsdStr = usd2Rur.ToString("#,#.##", new CultureInfo("ru-RU"));
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
