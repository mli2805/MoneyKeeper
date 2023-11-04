using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace Keeper2018
{
    public class CardBalanceMemoViewModel : PropertyChangedBase
    {
        private readonly KeeperDataModel _keeperDataModel;
        // public RangeObservableCollection<CardBalanceMemoModel> Rows { get; set; } = new RangeObservableCollection<CardBalanceMemoModel>();
        public ObservableCollection<CardBalanceMemoModel> Rows { get; set; } = new ObservableCollection<CardBalanceMemoModel>();

        public CardBalanceMemoViewModel(KeeperDataModel keeperDataModel)
        {
            _keeperDataModel = keeperDataModel;
        }

        public void Initialize()
        {
            // temp
            // var joker = _keeperDataModel.AcMoDict[781];
            // _keeperDataModel.CardBalanceMemoModels.Add(new CardBalanceMemoModel(){Id = 1, Account = joker, BalanceThreshold = 150, CurrentBalance = 200 });
            // var shopper = _keeperDataModel.AcMoDict[878];
            // _keeperDataModel.CardBalanceMemoModels.Add(new CardBalanceMemoModel(){Id = 1, Account = shopper, BalanceThreshold = 120, CurrentBalance = 100 });
            //--------------------

            Rows.Clear();
            // Rows.AddRange(_keeperDataModel.CardBalanceMemoModels);
            _keeperDataModel.CardBalanceMemoModels.ForEach(m => Rows.Add(m));


        }

    }
}
