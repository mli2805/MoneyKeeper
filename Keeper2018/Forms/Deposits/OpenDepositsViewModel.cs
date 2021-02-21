using System;
using System.Collections.Generic;
using Caliburn.Micro;

namespace Keeper2018
{
    public class DepositVm
    {
        public int Id { get; set; }
        public string BankName { get; set; }
        public string DepoName { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }

        public Balance Balance { get; set; }
    }
    public class OpenDepositsViewModel : Screen
    {
        private readonly KeeperDataModel _keeperDataModel;
        public List<DepositVm> Rows { get; set; }

        public OpenDepositsViewModel(KeeperDataModel keeperDataModel)
        {
            _keeperDataModel = keeperDataModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Действующие депозиты";
        }

        public void Initialize()
        {
            var depoFolder = _keeperDataModel.AcMoDict[166];

        }
    }
}
