using Caliburn.Micro;

namespace Keeper2018
{
    public class TagAssociationsViewModel : Screen
    {
        public KeeperDb KeeperDb { get; }

        public TagAssociationsViewModel(KeeperDb keeperDb)
        {
            KeeperDb = keeperDb;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Tag associations";
        }
    }
}
