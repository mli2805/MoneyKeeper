using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace Keeper2018
{
    public class CardBalanceMemoViewModel : PropertyChangedBase
    {
        private readonly KeeperDataModel _keeperDataModel;
        public ObservableCollection<CardBalanceMemoModel> Rows { get; set; } = new ObservableCollection<CardBalanceMemoModel>();

        public CardBalanceMemoViewModel(KeeperDataModel keeperDataModel)
        {
            _keeperDataModel = keeperDataModel;
        }

        public void Initialize()
        {
            Rows.Clear();
            _keeperDataModel.CardBalanceMemoModels.ForEach(m => Rows.Add(m));


        }

    }
}
