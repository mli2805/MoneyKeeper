using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Caliburn.Micro;
using Keeper.ByFunctional;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
    [Export]
    class NbRatesViewModel : Screen
    {
        private readonly NbRbRatesExtractor _nbRbRatesExtractor;
        private readonly BackgroundWorker _backgroundWorker;
        private bool _isWorking;
        private string _clockContent;
        public string ClockContent
        {
            get { return _clockContent; }
            set
            {
                if (value == _clockContent) return;
                _clockContent = value;
                NotifyOfPropertyChange(() => ClockContent);
            }
        }

        public ObservableCollection<NbRate> Rows { get; set; }
        public NbRate Line { get; set; }

        private bool _buttonDownloadIsEnabled;

        public bool ButtonDownloadIsEnabled
        {
            get { return _buttonDownloadIsEnabled; }
            set
            {
                if (value.Equals(_buttonDownloadIsEnabled)) return;
                _buttonDownloadIsEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        [ImportingConstructor]
        public NbRatesViewModel(KeeperDb db, NbRbRatesExtractor nbRbRatesExtractor)
        {
            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimerTick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();

            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.DoWork += BackgroundWorkerDoWork;

            if (db.OfficialRates == null)
            {
                db.OfficialRates = new ObservableCollection<NbRate>();
                db.OfficialRates.Add(new NbRate() { Date = new DateTime(1995, 3, 31), UsdRate = 11550, EurRate = 0, RurRate = 2.28 });
            }

            Rows = db.OfficialRates;
            ButtonDownloadIsEnabled = true;

            _nbRbRatesExtractor = nbRbRatesExtractor;
            _isWorking = false;
        }

        private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            DownloadMissingRatesFromNbRbSite();
        }

        private void DispatcherTimerTick(object sender, EventArgs e)
        {
            ClockContent = String.Format("{0:HH:mm:ss}", DateTime.Now);
            CommandManager.InvalidateRequerySuggested();
        }

        public bool IsCollectionChanged { get; set; }
        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Курсы НБ РБ";
        }


        private void DownloadRatesFromNbRbSiteForDate(DateTime date)
        {
            var dateRates = _nbRbRatesExtractor.GetRatesForDate(date);
            if (dateRates == null)
            {
                MessageBox.Show(String.Format("Курсы за {0} недоступны", date));
                return;
            }
            Line = new NbRate() { Date = date };
            foreach (var rate in dateRates)
            {
                switch (rate.Key)
                {
                    case CurrencyCodes.USD:
                        Line.UsdRate = date < new DateTime(2000,1,1) ? rate.Value / 1000 : rate.Value;
                        break;
                    case CurrencyCodes.EUR:
                        Line.EurRate = date < new DateTime(2000, 1, 1) ? rate.Value / 1000 : rate.Value;
                        break;
                    case CurrencyCodes.RUB:
                        Line.RurRate = date < new DateTime(2000, 1, 1) ? rate.Value / 1000 : rate.Value;
                        break;
                    default:
                        MessageBox.Show(string.Format("Неизвестная валюта {0}", rate.Key));
                        break;
                }
            }
            Execute.OnUIThread(UiThreadWork);
        }

        private void UiThreadWork()
        {
            Rows.Add(Line);
        }

        private void DownloadMissingRatesFromNbRbSite()
        {
            ButtonDownloadIsEnabled = false;
            var lastLine = Rows.LastOrDefault();
            var lastDate = lastLine == null ? new DateTime(1995, 3, 31) : lastLine.Date;

            while (lastDate.Date <= DateTime.Today.Date)
            {
                DownloadRatesFromNbRbSiteForDate(lastDate.AddDays(1));
                if (!_isWorking) break;
                lastDate = lastDate.AddDays(1);
            }
            ButtonDownloadIsEnabled = true;
        }

        public void Download()
        {
            _isWorking = true;
            _backgroundWorker.RunWorkerAsync();
        }

        public void Close()
        {
            _isWorking = false;
            TryClose();
        }
    }
}
