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

        public List<string> EssentialDates { get; set; }


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
            EssentialDates = ModelInWork.ConditionsMap.Keys.Select(d => d.ToString(_dateTemplate)).ToList();
            // EssentialDates = ModelInWork.ConditionsMap.Keys.ToList();
            if (EssentialDates.Count > 0) SelectedDate = EssentialDates.Last();
            //  if (EssentialDates.Count > 0) SelectedDate = ModelInWork.ConditionsMap.Keys.First();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Банковский депозит";
        }

        public void AddEssentials()
        {
            var date = DateTime.Today;
            while (ModelInWork.ConditionsMap.ContainsKey(date)) date = date.AddDays(1);

            var maxId = ModelInWork.ConditionsMap.Values.Max(e => e.Id);
            var depositEssential = new DepositConditions() { Id = maxId + 1 };
            _rulesAndRatesViewModel.Initialize(ModelInWork.Title, date, depositEssential);
            _windowManager.ShowDialog(_rulesAndRatesViewModel);
            ModelInWork.ConditionsMap.Add(_rulesAndRatesViewModel.SelectedDate, depositEssential);
            EssentialDates = ModelInWork.ConditionsMap.Keys.Select(d => d.ToString(_dateTemplate)).ToList();
            NotifyOfPropertyChange(nameof(EssentialDates));
        }

        public void EditEssentials()
        {
            if (SelectedDate == null) return;
            var date = DateTime.ParseExact(SelectedDate, _dateTemplate, new DateTimeFormatInfo());
            _rulesAndRatesViewModel.Initialize(ModelInWork.Title, date, ModelInWork.ConditionsMap[date]);
            _windowManager.ShowDialog(_rulesAndRatesViewModel);
            if (date == _rulesAndRatesViewModel.SelectedDate) return;

            var essentials = ModelInWork.ConditionsMap[date];
            ModelInWork.ConditionsMap.Remove(date);
            ModelInWork.ConditionsMap.Add(_rulesAndRatesViewModel.SelectedDate, essentials);

            EssentialDates = ModelInWork.ConditionsMap.Keys.Select(d => d.ToString(_dateTemplate)).ToList();
            NotifyOfPropertyChange(nameof(EssentialDates));
        }

        public void RemoveEssentials()
        {
            if (SelectedDate == null) return;
            var date = DateTime.ParseExact(SelectedDate, _dateTemplate, new DateTimeFormatInfo());

            ModelInWork.ConditionsMap.Remove(date);

            EssentialDates = ModelInWork.ConditionsMap.Keys.Select(d => d.ToString(_dateTemplate)).ToList();
            NotifyOfPropertyChange(nameof(EssentialDates));
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
