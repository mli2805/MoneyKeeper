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
        private readonly KeeperDataModel _keeperDataModel;
        private readonly IWindowManager _windowManager;
        private readonly RulesAndRatesViewModel _rulesAndRatesViewModel;
        public List<AccountModel> Banks { get; set; }
        public List<CurrencyCode> Currencies { get; set; }
        public DepositOfferModel ModelInWork { get; set; }

        public List<string> ConditionDates { get; set; }


        public string SelectedDate { get; set; }

        public bool IsCancelled { get; set; }

        public OneBankOfferViewModel(KeeperDataModel keeperDataModel, IWindowManager windowManager, RulesAndRatesViewModel rulesAndRatesViewModel)
        {
            _keeperDataModel = keeperDataModel;
            _windowManager = windowManager;
            _rulesAndRatesViewModel = rulesAndRatesViewModel;
        }

        public void Initialize(DepositOfferModel model)
        {
            Banks = _keeperDataModel.AcMoDict[220].Children;
            Currencies = Enum.GetValues(typeof(CurrencyCode)).OfType<CurrencyCode>().ToList();
            ModelInWork = model;
            ConditionDates = ModelInWork.CondsMap.Keys.Select(d => d.ToString(_dateTemplate)).ToList();
            if (ConditionDates.Count > 0) SelectedDate = ConditionDates.Last();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Банковский депозит";
        }

        public void AddConditions()
        {
            var date = DateTime.Today;
            while (ModelInWork.CondsMap.ContainsKey(date)) date = date.AddDays(1); // че та не работает

            var lastIdInDb = _keeperDataModel.GetDepoConditionsMaxId();
            var lastIdHere = ModelInWork.CondsMap.Any() 
                ? ModelInWork.CondsMap.Values.ToList().Max(c=>c.Id) 
                : 0;
            var maxId = Math.Max(lastIdInDb, lastIdHere);

            // var depositConditions = new DepositConditions(maxId + 1, ModelInWork.Id, DateTime.Today);
            var depoCondsModel = new DepoCondsModel()
            {
                Id = maxId + 1, 
                DepositOfferId = ModelInWork.Id,
                DateFrom = DateTime.Today,
            };
            _rulesAndRatesViewModel.Initialize(ModelInWork.Title, depoCondsModel, _keeperDataModel, ModelInWork.CondsMap.Keys.ToList());
            _windowManager.ShowDialog(_rulesAndRatesViewModel);
            ModelInWork.CondsMap.Add(depoCondsModel.DateFrom, depoCondsModel);
            ConditionDates = ModelInWork.CondsMap.Keys.Select(d => d.ToString(_dateTemplate)).ToList();
            NotifyOfPropertyChange(nameof(ConditionDates));
        }

        public void EditConditions()
        {
            if (SelectedDate == null) return;
            var date = DateTime.ParseExact(SelectedDate, _dateTemplate, new DateTimeFormatInfo());
            _rulesAndRatesViewModel.Initialize(ModelInWork.Title, ModelInWork.CondsMap[date], _keeperDataModel,
                ModelInWork.CondsMap.Keys.ToList());
            _windowManager.ShowDialog(_rulesAndRatesViewModel);
            if (date == ModelInWork.CondsMap[date].DateFrom) return;

            var depoCondsModel = ModelInWork.CondsMap[date];
            ModelInWork.CondsMap.Remove(date);
            ModelInWork.CondsMap.Add(depoCondsModel.DateFrom, depoCondsModel);

            ConditionDates = ModelInWork.CondsMap.Keys.Select(d => d.ToString(_dateTemplate)).ToList();
            NotifyOfPropertyChange(nameof(ConditionDates));
        }

        public void RemoveConditions()
        {
            if (SelectedDate == null) return;
            var date = DateTime.ParseExact(SelectedDate, _dateTemplate, new DateTimeFormatInfo());

            ModelInWork.CondsMap.Remove(date);

            ConditionDates = ModelInWork.CondsMap.Keys.Select(d => d.ToString(_dateTemplate)).ToList();
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
