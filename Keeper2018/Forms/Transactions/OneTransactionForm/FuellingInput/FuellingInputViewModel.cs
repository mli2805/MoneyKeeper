using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class FuellingInputViewModel : Screen
    {
        private readonly KeeperDb _db;
        public Fuelling Vm { get; set; }

        public List<CurrencyCode> Currencies { get; set; } = Enum.GetValues(typeof(CurrencyCode)).OfType<CurrencyCode>().ToList();
        public List<FuelType> Fuels { get; set; } = Enum.GetValues(typeof(FuelType)).OfType<FuelType>().ToList();

        public List<string> Cars { get; set; }

        private string _selectedCar;
        public string SelectedCar
        {
            get => _selectedCar;
            set
            {
                if (value == _selectedCar) return;
                _selectedCar = value;
                NotifyOfPropertyChange();
            }
        }

        public FuellingInputViewModel(KeeperDb db)
        {
            _db = db;
        }

        public void Initialize(Fuelling vm)
        {
            Cars = _db.Bin.Cars.Select(c => c.Title).ToList();
            Vm = (Fuelling) vm.Clone();
            SelectedCar = _db.Bin.Cars.First(c => c.AccountId == vm.CarAccountId).Title;
        }
        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Ввод заправки";
        }

        public void Save()
        {
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }
    }
}
