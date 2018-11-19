using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class OneDepositOfferViewModel : Screen
    {
        private readonly KeeperDb _keeperDb;
        public List<Account> Banks { get; set; }
        public DepositOfferModel ModelInWork { get; set; }
        public List<DateTime> EssentialDates { get;set; }
        public DateTime SelectedDate { get; set; }

        public bool IsCancelled { get; set; }

        public OneDepositOfferViewModel(KeeperDb keeperDb)
        {
            _keeperDb = keeperDb;
        }

        public void Initialize(DepositOfferModel model)
        {
            var bankFolder = _keeperDb.Bin.AccountPlaneList.First(a => a.Name == "Банки");
            Banks = new List<Account>(_keeperDb.Bin.AccountPlaneList.Where(a=>a.OwnerId == bankFolder.Id));

            ModelInWork = model == null ? new DepositOfferModel() : model.DeepCopy();
            EssentialDates = ModelInWork.Essentials.Keys.ToList();
         //   SelectedDate = null;
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
