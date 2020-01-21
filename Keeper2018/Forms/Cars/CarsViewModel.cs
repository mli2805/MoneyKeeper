using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Caliburn.Micro;

namespace Keeper2018
{
    public class CarsViewModel : Screen
    {
        private readonly KeeperDb _db;
        private readonly IWindowManager _windowManager;
        private readonly FuelViewModel _fuelViewModel;

        public List<CarVm> Cars { get; set; }

        private CarVm _selectedCar;

        public CarVm SelectedCar
        {
            get { return _selectedCar; }
            set
            {
                if (Equals(value, _selectedCar)) return;
                _selectedCar = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsLastCarVisibility));
            }
        }

        public Visibility IsLastCarVisibility => SelectedCar.AccountId == Cars.Last().AccountId
            ? Visibility.Visible : Visibility.Collapsed;

        public CarsViewModel(KeeperDb db, IWindowManager windowManager, FuelViewModel fuelViewModel)
        {
            _db = db;
            _windowManager = windowManager;
            _fuelViewModel = fuelViewModel;
        }

        public void Initialize()
        {
            if (_db.Bin.Cars == null)
                _db.Bin.Cars = InitializeBase();

            Cars = new List<CarVm>();
            foreach (var car in _db.Bin.Cars)
            {
                Cars.Add(new CarVm(car));
            }

            SelectedCar = Cars.Last();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Автомобили";
        }

        public void AddNewCar()
        {

        }

        public void Fuelling()
        {
            _fuelViewModel.Initialize();
            _windowManager.ShowWindow(_fuelViewModel);
        }

      
     
        public void ShowCarReport()
        {
            if (SelectedCar.AccountId < 711) return;
            var provider = new CarReportProvider(_db);
            var document = provider.CreateCarReport(SelectedCar.AccountId);

            try
            {
                string filename = $@"{SelectedCar.Title}.pdf";
                var path = DbIoUtils.GetReportFullPath(filename);
                document.Save(path);
                Process.Start(path);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public void Close() { TryClose(); }

        private static List<Car> InitializeBase()
        {
            return new List<Car>
            {
                new Car{AccountId = 706, Title = "VW Golf II 1,6 TD", IssueYear = 1991, StateRegNumber = "3670 PC",
                    Start = new DateTime(1998,7,15), MileageStart = 146800,
                    Finish = new DateTime(2006,11,15), MileageFinish = 259300,
                    Comment = "Купил в Касселе, пригнал 146800 - 148400",
                    YearMileages = new [] { 156400, 166400,176400, 193400, 207400, 221400, 235400, 250400,},
                },


                new Car{AccountId = 708, Title = "VW Passat B4 1,9 TDI", IssueYear = 1996, StateRegNumber = "9051 АР-7",
                    Start = new DateTime(2006,10,7), MileageStart = 277100,
                    Finish = new DateTime(2009,09,23), MileageFinish = 317700,
                    YearMileages = new []{290100, 308600,},
                },

                new Car{AccountId = 711, Title = "Renault Grand Scenic II 1,9 dCi", IssueYear = 2005,
                    StateRegNumber = "9734 КА-7", Vin = "VF1JM1GE634636175",
                    Start = new DateTime(2009,4,2), MileageStart = 157900,
                    Finish = new DateTime(2014,9,3), MileageFinish = 256150,
                    YearMileages = new []{178300, 203300, 225400, 239400, 254200, },
                },

                new Car{AccountId = 716, Title = "Renault Grand Scenic III 1,5 dCi", IssueYear = 2010,
                    StateRegNumber = "8688 НК-7", Vin = "VF1JZ1GB642744065",
                    Start = new DateTime(2014,4,14), MileageStart = 134750,
                    Finish = DateTime.Today, MileageFinish = 199800, // на сегодня
                    YearMileages = new []{145750, 159750, 173750, 188250, 199750, },
                },
            };
        }
    }
}
