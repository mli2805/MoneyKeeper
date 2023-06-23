using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class RefinancingRatesViewModel : PropertyChangedBase
    {
        private readonly KeeperDataModel _keeperDataModel;
        public ObservableCollection<RefinancingRate> Rows { get; set; }

        public RefinancingRatesViewModel(KeeperDataModel keeperDataModel)
        {
            _keeperDataModel = keeperDataModel;
        }

        public void Initialize()
        {
            Rows = new ObservableCollection<RefinancingRate>(_keeperDataModel.RefinancingRates);
        }

        public async void Download()
        {
            var rates = await NbRbRatesDownloader.GetRefinanceRatesAsync();
            if (rates == null) return;

            if (rates.Count > _keeperDataModel.RefinancingRates.Count)
            {
                var lastId = 0;
                foreach (var refinancingRate in rates.OrderBy(r => r.Date))
                {
                    var dr = _keeperDataModel.RefinancingRates.FirstOrDefault(r => r.Date == refinancingRate.Date);
                    if (dr != null)
                        lastId = dr.Id;
                    else
                    {
                        refinancingRate.Id = ++lastId;
                        _keeperDataModel.RefinancingRates.Add(refinancingRate);
                        Rows.Add(refinancingRate);
                    }
                }
            }
        }

        public void UpdateRates()
        {
            _keeperDataModel.UpdateDepoRatesLinkedToCp();
        }

    }
}
