using System;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class DepositReportViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        public DepositReportModel Model { get; set; }

        public DepositReportViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Initialize(AccountItemModel accountItemModel)
        {
            var accountCalculator = 
                new TrafficOfAccountCalculator(_dataModel, accountItemModel, 
                    new Period(new DateTime(2001, 12, 31), DateTime.Today.GetEndOfDate()));
            accountCalculator.EvaluateAccount();
            Model = accountCalculator.DepositReportModel;
            Model.SummarizeFacts(_dataModel);
            if (Model.AmountInUsd != 0)
            {
                Model.Foresee(_dataModel);
            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Model.DepositName;
        }

        public void Close() { TryClose(); }
    }
}
