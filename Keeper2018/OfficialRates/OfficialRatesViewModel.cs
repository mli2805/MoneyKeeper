using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

namespace Keeper2018
{
    public class OfficialRatesViewModel : Screen
    {
        public string Clock { get; set; } = "clock";
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

        public void Close()
        {
            TryClose();
        }
    }
}
