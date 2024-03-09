namespace Keeper2018
{
    public class LargeExpenseThresholdViewModel
    {
        public KeeperDataModel KeeperDataModel { get; set; }

        public LargeExpenseThresholdViewModel(KeeperDataModel keeperDataModel)
        {
            KeeperDataModel = keeperDataModel;
        }
    }
}
