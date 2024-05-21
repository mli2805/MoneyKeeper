using System.Collections.Generic;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Keeper2018
{
    public class CardBalanceMemoViewModel : PropertyChangedBase
    {
        private readonly KeeperDataModel _keeperDataModel;
        public List<CardBalanceMemoModel> Rows { get; set; } = new List<CardBalanceMemoModel>();

        public CardBalanceMemoViewModel(KeeperDataModel keeperDataModel)
        {
            _keeperDataModel = keeperDataModel;
        }

        public async Task Initialize()
        {
            Rows.Clear();
            await _keeperDataModel.RefreshCardBalances();
            Rows.AddRange(_keeperDataModel.CardBalanceMemoModels);
        }

    }
}
