using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class OneBankOfferViewModel : Screen
    {
        private readonly string _dateTemplate = "dd-MM-yyyy";
        private readonly KeeperDb _keeperDb;
        private readonly IWindowManager _windowManager;
        private readonly RulesAndRatesViewModel _rulesAndRatesViewModel;
        public List<Account> Banks { get; set; }
        public List<CurrencyCode> Currencies { get; set; }
        public DepositOfferModel ModelInWork { get; set; }

        public List<string> ConditionDates { get; set; }


        public string SelectedDate { get; set; }

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
            Currencies = Enum.GetValues(typeof(CurrencyCode)).OfType<CurrencyCode>().ToList();
            ModelInWork = model;
            ConditionDates = ModelInWork.ConditionsMap.Keys.Select(d => d.ToString(_dateTemplate)).ToList();
            if (ConditionDates.Count > 0) SelectedDate = ConditionDates.Last();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Банковский депозит";
        }

        public void AddConditions()
        {
            var date = DateTime.Today;
            while (ModelInWork.ConditionsMap.ContainsKey(date)) date = date.AddDays(1);

            var lastIdInDb = _keeperDb.Bin.DepositOffers
                .SelectMany(depositOffer => depositOffer.ConditionsMap.Values)
                .ToList()
                .Max(c => c.Id);
            var lastIdHere = ModelInWork.ConditionsMap.Any() 
                ? ModelInWork.ConditionsMap.Values.ToList().Max(c=>c.Id) 
                : 0;
            var maxId = Math.Max(lastIdInDb, lastIdHere);

            var depositConditions = new DepositConditions(maxId + 1, ModelInWork.Id, DateTime.Today);
            _rulesAndRatesViewModel.Initialize(ModelInWork.Title, depositConditions);
            _windowManager.ShowDialog(_rulesAndRatesViewModel);
            ModelInWork.ConditionsMap.Add(depositConditions.DateFrom, depositConditions);
            ConditionDates = ModelInWork.ConditionsMap.Keys.Select(d => d.ToString(_dateTemplate)).ToList();
            NotifyOfPropertyChange(nameof(ConditionDates));
        }

        public void EditConditions()
        {
            if (SelectedDate == null) return;
            var date = DateTime.ParseExact(SelectedDate, _dateTemplate, new DateTimeFormatInfo());
            _rulesAndRatesViewModel.Initialize(ModelInWork.Title, ModelInWork.ConditionsMap[date]);
            _windowManager.ShowDialog(_rulesAndRatesViewModel);
            if (date == ModelInWork.ConditionsMap[date].DateFrom) return;

            var conditions = ModelInWork.ConditionsMap[date];
            ModelInWork.ConditionsMap.Remove(date);
            ModelInWork.ConditionsMap.Add(conditions.DateFrom, conditions);

            ConditionDates = ModelInWork.ConditionsMap.Keys.Select(d => d.ToString(_dateTemplate)).ToList();
            NotifyOfPropertyChange(nameof(ConditionDates));
        }

        public void RemoveConditions()
        {
            if (SelectedDate == null) return;
            var date = DateTime.ParseExact(SelectedDate, _dateTemplate, new DateTimeFormatInfo());

            ModelInWork.ConditionsMap.Remove(date);

            ConditionDates = ModelInWork.ConditionsMap.Keys.Select(d => d.ToString(_dateTemplate)).ToList();
            NotifyOfPropertyChange(nameof(ConditionDates));
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
