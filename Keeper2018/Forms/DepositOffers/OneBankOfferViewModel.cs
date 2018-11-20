using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class OneBankOfferViewModel : Screen
    {
        private readonly KeeperDb _keeperDb;
        private readonly IWindowManager _windowManager;
        private readonly RulesAndRatesViewModel _rulesAndRatesViewModel;
        public List<Account> Banks { get; set; }
        public DepositOfferModel ModelInWork { get; set; }
        public List<string> EssentialDates { get; set; }
        public DateTime SelectedDate { get; set; }

        public bool IsCancelled { get; set; }

        public OneBankOfferViewModel(KeeperDb keeperDb, IWindowManager windowManager, RulesAndRatesViewModel rulesAndRatesViewModel)
        {
            _keeperDb = keeperDb;
            _windowManager = windowManager;
            _rulesAndRatesViewModel = rulesAndRatesViewModel;
        }

        public void Initialize(DepositOfferModel model)
        {
            var bankFolder = _keeperDb.Bin.AccountPlaneList.First(a => a.Name == "Банки");
            Banks = new List<Account>(_keeperDb.Bin.AccountPlaneList.Where(a => a.OwnerId == bankFolder.Id));

            ModelInWork = model == null ? new DepositOfferModel() {Bank = Banks.First(),} : model.DeepCopy();
            EssentialDates = ModelInWork.Essentials.Keys.Select(d=>d.ToString("dd-MM-yyyy")).ToList();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Банковский депозит";
        }

        public void Save()
        {
            IsCancelled = false;
            TryClose();
        }

        public void Cancel()
        {
            IsCancelled = true;
            TryClose();
        }

    }
}
