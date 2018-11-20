using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class DepositReportViewModel : Screen
    {
        private readonly KeeperDb _db;
        private AccountModel _accountModel;

        public DepositReportViewModel(KeeperDb db)
        {
            _db = db;
        }

        public void Initialize(AccountModel accountModel)
        {
            _accountModel = accountModel;
            var traffic = _db.Bin.Transactions.Where(t =>
                t.MyAccount == accountModel.Id || t.MySecondAccount == accountModel.Id).ToList();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _accountModel.Name;
        }
    }
}
