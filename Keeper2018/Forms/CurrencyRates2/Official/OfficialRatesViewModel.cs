using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class OfficialRatesViewModel : PropertyChangedBase
    {
        private readonly KeeperDataModel _keeperDataModel;
        public ObservableCollection<CurrencyRatesModel> Rows { get; set; } = new ObservableCollection<CurrencyRatesModel>();
    
        private CurrencyRatesModel _selectedRow;
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
        
        public OfficialRatesViewModel(KeeperDataModel keeperDataModel)
        {
            _keeperDataModel = keeperDataModel;
        }

        public async Task Initialize()
        {
            await Task.Factory.StartNew(Init);
            IsDownloadEnabled = true;
        }

        private void Init()
        {
            CurrencyRatesModel annual = null;
            CurrencyRatesModel previous = null;
            foreach (var record in _keeperDataModel.Rates)
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

        public async void Download()
        {
            IsDownloadEnabled = false;
            using (new WaitCursor())
            {
                var date = Rows.Last().Date.AddDays(1);
                var annual = Rows.Last(r => r.Date.Day == 31 && r.Date.Month == 12);
                while (date <= DateTime.Today.Date.AddDays(1))
                {
                    var currencyRates = await DownloadRbAndRfRates(date);
                    if (currencyRates == null) break;

                    currencyRates.Id = Rows.Last().Id + 1;
                    _keeperDataModel.Rates.Add(currencyRates.Date, currencyRates);
                    var line = new CurrencyRatesModel(currencyRates, Rows.Last(), annual);
                    Rows.Add(line);

                    if (date.Date.Day == 31 && date.Date.Month == 12)
                        annual = line;
                    date = date.AddDays(1);
                }
            }
            IsDownloadEnabled = true;
        }

        private async Task<CurrencyRates> DownloadRbAndRfRates(DateTime date)
        {
            var nbRbRates = await NbRbRatesDownloader.GetRatesForDate(date);
            if (nbRbRates == null) return null;
            var currencyRates = new CurrencyRates() { Date = date, NbRates = nbRbRates };
            var usd2Rur = await CbrRatesDownloader.GetRateForDateFromXml(date);
            currencyRates.CbrRate.Usd = new OneRate() { Unit = 1, Value = usd2Rur };
            return currencyRates;
        }


        public void RemoveLine()
        {
            _keeperDataModel.Rates.Remove(SelectedRow.Date);
            Rows.Remove(SelectedRow);
        }

    }
}
