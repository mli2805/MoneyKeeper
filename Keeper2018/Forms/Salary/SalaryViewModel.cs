using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class SalaryViewModel : Screen
    {
        private readonly KeeperDb _db;

        public SalaryViewModel(KeeperDb db)
        {
            _db = db;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Salary";
        }

        public void Initialize()
        {
            var mySalaryFolder = _db.AcMoDict[772];
            var lines = _db.Bin.Transactions.Where(t => t.Value.Tags.ToList().Intersect(mySalaryFolder.Children.Select(c=>c.Id)).Any());
        }
    }
}
