using System.Collections.ObjectModel;
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
        }

        public void Init()
        {
            if (Rows != null) return;
            Rows = new ObservableCollection<NbRbRateOnScreen>();
            NbRbRateOnScreen previous = null;
            foreach (var record in NbRbRatesOldTxt.LoadFromOldTxt())
            {
                var s = new NbRbRateOnScreen(record, previous);
                Application.Current.Dispatcher.Invoke(() => Rows.Add(s));
                previous = s;
            }
        }

        public void Close()
        {
            TryClose();
        }
    }
}
