using System;
using System.Collections.Generic;
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
        public RangeObservableCollection<OfficialRatesModel> Rows { get; set; } = new RangeObservableCollection<OfficialRatesModel>();

        private OfficialRatesModel _selectedRow;
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
        public OfficialRatesModel DayYearAgo { get; set; }

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
            OfficialRatesModel endOfLastYear = null;
            OfficialRatesModel yearAgo = null;
            OfficialRatesModel yesterday = null;

            List<OfficialRatesModel> data = new List<OfficialRatesModel>();

            foreach (var record in _keeperDataModel.OfficialRates)
            {
                yearAgo = data.LastOrDefault(d =>
                    d.Date.Day == record.Value.Date.Day && d.Date.Month == record.Value.Date.Month);
                var today = new OfficialRatesModel(record.Value, yesterday, endOfLastYear, yearAgo); // вычисления дельт
                data.Add(today);

                // if (Application.Current.Dispatcher != null)
                //     Application.Current.Dispatcher.Invoke(() => Rows.Add(current));

                // if (!today.BasketDelta.Equals(0))
                    yesterday = today;

                if (today.Date.Day == 31 && today.Date.Month == 12)
                    endOfLastYear = today;
            }

            if (Application.Current.Dispatcher != null)
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Rows.AddRange(data);
                    LastDayOfYear = endOfLastYear;
                    DayYearAgo = yearAgo;
                });
        }

        public async void Download()
        {
            IsDownloadEnabled = false;
            using (new WaitCursor())
            {
                var date = Rows.Last().Date.AddDays(1);
                var endOfLastYear = Rows.Last(r => r.Date.Day == 31 && r.Date.Month == 12);
                while (date <= DateTime.Today.Date.AddDays(1))
                {
                    var currencyRates = await DownloadRbAndRfRates(date);
                    if (currencyRates == null) break;

                    currencyRates.Id = Rows.Last().Id + 1;
                    _keeperDataModel.OfficialRates.Add(currencyRates.Date, currencyRates);
                    var yearAgo = Rows
                        .Last(r => r.Date.Day == currencyRates.Date.Day && r.Date.Month == currencyRates.Date.Month);
                    var line = new OfficialRatesModel(currencyRates, Rows.Last(), endOfLastYear, yearAgo);
                    Rows.Add(line);

                    if (date.Date.Day == 31 && date.Date.Month == 12)
                        endOfLastYear = line;
                    date = date.AddDays(1);
                }
            }
            IsDownloadEnabled = true;
        }

        private async Task<OfficialRates> DownloadRbAndRfRates(DateTime date)
        {
            var nbRbRates = await NbRbRatesDownloader.GetRatesForDateAsync(date);
            if (nbRbRates == null) return null;
            var currencyRates = new OfficialRates() { Date = date, NbRates = nbRbRates };
            var usd2Rur = await CbrRatesDownloader.GetRateForDateFromXml(date);
            currencyRates.CbrRate.Usd = new OneRate() { Unit = 1, Value = usd2Rur };
            return currencyRates;
        }


        public void RemoveLine()
        {
            _keeperDataModel.OfficialRates.Remove(SelectedRow.Date);
            Rows.Remove(SelectedRow);
        }

    }
}
