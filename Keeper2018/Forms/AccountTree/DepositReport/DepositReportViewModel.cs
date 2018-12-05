using System;
using Caliburn.Micro;

namespace Keeper2018
{
    public class DepositReportViewModel : Screen
    {
        private readonly KeeperDb _db;
        public DepositReportModel Model { get; set; }

        public DepositReportViewModel(KeeperDb db)
        {
            _db = db;
        }

        public void Initialize(AccountModel accountModel)
        {
            var accountCalculator = 
                new TrafficOfAccountCalculator(_db, accountModel, 
                    new Period(new DateTime(2001, 12, 31), DateTime.Today.GetEndOfDate()));
            accountCalculator.Evaluate();
            Model = accountCalculator.DepositReportModel;
            Model.Foresee(_db);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Model.DepositName;
        }

        public void Close() { TryClose(); }
    }
}
