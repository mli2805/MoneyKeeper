using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.Utils;

namespace Keeper.ViewModels.SingleViews
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
            ClockContent = $"{DateTime.Now:HH:mm:ss}";
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
                MessageBox.Show($"Курсы за {date} недоступны");
                return;
            }
            Line = new NbRate() { Date = date };
            foreach (var rate in dateRates)
            {
                switch (rate.Key)
                {
                    case CurrencyCodes.USD:
                        Line.UsdRate = ConsiderDenominations(date, rate);
                        break;
                    case CurrencyCodes.EUR:
                        Line.EurRate = ConsiderDenominations(date, rate);
                        break;
                    case CurrencyCodes.RUB:
                        Line.RurRate = ConsiderDenominations(date, rate);
                        break;
                    default:
                        MessageBox.Show($"Неизвестная валюта {rate.Key}");
                        break;
                }
            }
            Execute.OnUIThread(UiThreadWork);
        }

        private static double ConsiderDenominations(DateTime date, KeyValuePair<CurrencyCodes, double> rate)
        {
            return date < new DateTime(2000,1,1) ? rate.Value / 1000 :
                date < new DateTime(2016, 7, 1) ? rate.Value :
                rate.Key == CurrencyCodes.RUB ? rate.Value * 100 : rate.Value * 10000;
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

        public void ButtonClose()
        {
            _isWorking = false;
            TryClose();
        }
    }
}
