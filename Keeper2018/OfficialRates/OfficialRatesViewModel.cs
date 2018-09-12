﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

namespace Keeper2018
{
    public class OfficialRatesViewModel : Screen
    {
        public ObservableCollection<NbRbRateOnScreen> Rows { get; set; }
        public NbRbRateOnScreen SelectedRow { get; set; }
        public OfficialRatesViewModel()
        {
            Task.Factory.StartNew(Init);
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

        public async void Download()
        {
            var date = Rows.Last().Date.AddDays(1);
            var annual = Rows.Last(r => r.Date.Day == 31 && r.Date.Month == 12);
            while (date <= DateTime.Today.Date.AddDays(1))
            {
                var ratesFromSite = await OfficialRatesDownloader.GetRatesForDate(date);
                var nbRbRate = new NbRbRate(){Date = date, Values = ratesFromSite};
                var line = new NbRbRateOnScreen(nbRbRate, Rows.Last(), annual);
                Rows.Add(line);

                if (date.Date.Day == 31 && date.Date.Month == 12)
                    annual = line;
                date = date.AddDays(1);
            }
        }

        
        public void Close()
        {
            TryClose();
        }
    }
}
