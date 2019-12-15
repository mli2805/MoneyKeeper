using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Caliburn.Micro;

namespace Keeper2018
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Fuelling, FuellingInputVm>();
        }
    }
    public class FuellingInputViewModel : Screen
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();


        public int Top { get; set; }
        private int _left;
        public int Left
        {
            get { return _left; }
            set
            {
                if (value == _left) return;
                _left = value;
                NotifyOfPropertyChange();
            }
        }
        public int Height { get; set; }

        private readonly KeeperDb _db;
        public FuellingInputVm Vm { get; set; }

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
            Vm = Mapper.Map<Fuelling, FuellingInputVm>(vm);
            Vm.Db = _db;
            SelectedCar = _db.Bin.Cars.First(c => c.AccountId == vm.CarAccountId).Title;
        }
        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Ввод заправки";
        }

        public void PlaceIt(int top, int left, int height)
        {
            Top = top;
            Left = left;
            Height = height;
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
