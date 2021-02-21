using System;
using System.Collections.Generic;
using System.Linq;
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
            Rows = _keeperDataModel.AcMoDict.Values
                .Where(a => !a.Children.Any() && a.Is(166) && !a.Is(235))
                .OrderBy(d=>d.Deposit.FinishDate)
                .Select(Convert).ToList();
        }

        private DepositVm Convert(AccountModel accountModel)
        {
            var depoOffer = _keeperDataModel.DepositOffers
                .First(o => o.Id == accountModel.Deposit.DepositOfferId);
            var calc = new TrafficOfAccountCalculator(_keeperDataModel, accountModel, 
                new Period(accountModel.Deposit.StartDate, DateTime.Today));
            return new DepositVm()
            {
                Id = accountModel.Id,
                BankName = depoOffer.Bank.Name,
                DepoName = accountModel.Name,
                StartDate = accountModel.Deposit.StartDate,
                FinishDate = accountModel.Deposit.FinishDate,
                Balance = calc.EvaluateBalance(),
            };
        }
    }
}
