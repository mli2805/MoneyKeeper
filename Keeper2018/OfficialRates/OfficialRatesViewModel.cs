using System;
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
                if (!current.Delta.Equals(0))
                    previous = current;
                if (current.Date.DayOfYear == 1)
                    annual = current;
            }
        }

        public async void Download()
        {
            var date = Rows.Last().Date.AddDays(1);
            var annual = Rows.Last(r => r.Date.DayOfYear == 1);
            while (date <= DateTime.Today.Date)
            {
                var ratesFromSite = await OfficialRatesDownloader.GetRatesForDate(date);
                var nbRbRate = new NbRbRate(){Date = date, Values = ratesFromSite};
                var line = new NbRbRateOnScreen(nbRbRate, Rows.Last(), annual);
                Rows.Add(line);

                date = date.AddDays(1);
            }
        }

        
        public void Close()
        {
            TryClose();
        }
    }
}
