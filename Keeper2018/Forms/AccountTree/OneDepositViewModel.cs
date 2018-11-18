using System;
using Caliburn.Micro;

namespace Keeper2018
{
    public class OneDepositViewModel : Screen
    {
        public Deposit DepositInWork { get; set; }
        private string _windowTitle;
        private readonly KeeperDb _db;
        private readonly IWindowManager _windowManager;

        public OneDepositViewModel(KeeperDb db, IWindowManager windowManager)
        {
            _db = db;
            _windowManager = windowManager;
        }

        public void InitializeForm(Deposit deposit, string windowTitle)
        {
            _windowTitle = windowTitle;
            DepositInWork = deposit;

            if (windowTitle == "Добавить")
            {
  //              DepositInWork.DepositOffer = _db.OfferModels.First();
                DepositInWork.StartDate = DateTime.Today;
                DepositInWork.FinishDate = DateTime.Today.AddMonths(1);
            }

        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _windowTitle;
        }

        public void SaveDeposit()
        {
            TryClose(true);
        }

        public void CompileAccountName()
        {
//            var rate = DepositInWork.DepositOffer.RateLines == null || DepositInWork.DepositOffer.RateLines.LastOrDefault() == null
//                ? 0
//                : DepositInWork.DepositOffer.RateLines.Last().Rate;
//            Junction =
//                $"{DepositInWork.DepositOffer.BankAccount.Name} {DepositInWork.DepositOffer.DepositTitle} {DepositInWork.StartDate.ToString("d/MM/yyyy", CultureInfo.InvariantCulture)} - {DepositInWork.FinishDate.ToString("d/MM/yyyy", CultureInfo.InvariantCulture)} {rate:0.#}%";
        }

        public void FillDepositRatesTable()
        {
//            var bankDepositRatesAndRulesViewModel = IoC.Get<BankDepositRatesAndRulesViewModel>();
//            bankDepositRatesAndRulesViewModel.Initialize(DepositInWork.DepositOffer);
//            _windowManager.ShowDialog(bankDepositRatesAndRulesViewModel);
        }
    }
}
