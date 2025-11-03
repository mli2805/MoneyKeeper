using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class CarsViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private readonly IWindowManager _windowManager;
        private readonly FuelViewModel _fuelViewModel;
        private readonly OwnershipCostViewModel _ownershipCostViewModel;

        public List<CarModel> Cars { get; set; }

        private CarModel _selectedCar;

        public CarModel SelectedCar
        {
            get => _selectedCar;
            set
            {
                if (Equals(value, _selectedCar)) return;
                _selectedCar = value;
                YearMileagesToShow = new List<YearMileageModel>(_selectedCar.YearsMileage);
                EvaluateYearMileageToShow();
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsLastCarVisibility));
                NotifyOfPropertyChange(nameof(YearMileagesToShow));
                NotifyOfPropertyChange(nameof(Total));
                NotifyOfPropertyChange(nameof(TotalPlus));
            }
        }

        public List<YearMileageModel> YearMileagesToShow { get; set; }
        public YearMileageModel Total { get; set; }
        public YearMileageModel TotalPlus { get; set; }

        public Visibility IsLastCarVisibility => SelectedCar.Id == Cars.Last().Id
            ? Visibility.Visible : Visibility.Collapsed;

        public CarsViewModel(KeeperDataModel dataModel, IWindowManager windowManager,
            FuelViewModel fuelViewModel, OwnershipCostViewModel ownershipCostViewModel)
        {
            _dataModel = dataModel;
            _windowManager = windowManager;
            _fuelViewModel = fuelViewModel;
            _ownershipCostViewModel = ownershipCostViewModel;
        }

        public void Initialize()
        {
            _dataModel.Cars.Last().SaleDate = DateTime.Today;

            Cars = _dataModel.Cars;
            SelectedCar = Cars.Last();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Автомобили";
        }

        private void EvaluateYearMileageToShow()
        {
            var prevOdometer = SelectedCar.PurchaseMileage;
            // меняем отдельную копию , а не то что хранится в базе
            for (int i = 0; i < YearMileagesToShow.Count; i++)
            {
                var yearMileageModel = YearMileagesToShow[i];
                Period period = new Period(SelectedCar.PurchaseDate.AddYears(i),
                    SelectedCar.PurchaseDate.AddYears(i + 1).AddDays(-1));
                if (period.FinishMoment > SelectedCar.SaleDate) period.FinishMoment = SelectedCar.SaleDate;
                yearMileageModel.YearNumber = i + 1;
                yearMileageModel.Period = period;

                yearMileageModel.Mileage = yearMileageModel.Odometer - prevOdometer;
                prevOdometer = yearMileageModel.Odometer;

                EvaluateAmount(yearMileageModel);
            }

            if (SelectedCar == Cars.Last())
            {
                var lastYear = YearMileagesToShow.Last();
                if (lastYear.Period.FinishMoment.Date < DateTime.Today)
                {
                    var currentYear = new YearMileageModel()
                    {
                        CarId = SelectedCar.CarAccountId,
                        Period = new Period(lastYear.Period.FinishMoment.Date.AddDays(1), DateTime.Today),
                        YearNumber = lastYear.YearNumber + 1,
                        Odometer = SelectedCar.SaleMileage,
                        Mileage = SelectedCar.SaleMileage - prevOdometer
                    };
                    EvaluateAmount(currentYear);
                    YearMileagesToShow.Add(currentYear);
                }
            }

            var fullPeriod = new Period(SelectedCar.PurchaseDate, YearMileagesToShow.Last().Period.FinishMoment);
            Total = new YearMileageModel()
            {
                Mileage = YearMileagesToShow.Sum(y => y.Mileage),
                Period = fullPeriod,
                YearAmount = YearMileagesToShow.Sum(y => y.YearAmount),
            };
            Total.DayAmount = Total.YearAmount / fullPeriod.ToDays();
            TotalPlus = new YearMileageModel()
            {
                CarId = SelectedCar.CarAccountId,
                Mileage = YearMileagesToShow.Sum(y => y.Mileage),
                Period = fullPeriod,
                YearAmount = YearMileagesToShow.Sum(y => y.YearAmount),
            };
            EvaluateAmount(TotalPlus, true);
        }

        private void EvaluateAmount(YearMileageModel yearMileageModel, bool includePurchase = false)
        {
            yearMileageModel.YearAmount = _dataModel.Transactions.Values
                .Where(t => yearMileageModel.Period.Includes(t.Timestamp) &&
                            t.Operation == OperationType.Расход &&
                            t.Category.Parent.Is(SelectedCar.CarAccountId) &&
                            (t.Tags.All(tag => tag.Id != 1064) || includePurchase)) // тэг покупки-продажи авто
                .Sum(t => t.GetAmountInUsd(_dataModel));

            if (yearMileageModel.CarId == Cars.Last().CarAccountId && includePurchase)
            {
                yearMileageModel.YearAmount -= SelectedCar.SupposedSalePrice;
            }

            yearMileageModel.DayAmount = yearMileageModel.YearAmount / yearMileageModel.Period.ToDays();
        }

        public void AddNewCar()
        {

        }

        public void Fuelling()
        {
            _fuelViewModel.Initialize();
            _windowManager.ShowWindow(_fuelViewModel);
        }

        public bool IsByTags { get; set; }
        public bool IsBynInReport { get; set; }
        public void ShowCarReport()
        {
            if (SelectedCar.Id < 3) return;
            var document = _dataModel.CreateCarReport(SelectedCar.Id, IsByTags, IsBynInReport);

            try
            {
                string filename = $@"{SelectedCar.Title}.pdf";
                var path = PathFactory.GetReportFullPath(filename);
                document.Save(path);
                Process.Start(path);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void ShowOwnershipCostChart()
        {
            _ownershipCostViewModel.Initialize(_selectedCar);
            _windowManager.ShowDialog(_ownershipCostViewModel);
        }

        public override void CanClose(Action<bool> callback)
        {
            Save();
            base.CanClose(callback);
        }

        public void Close()
        {
            TryClose();
        }

        private void Save()
        {
            var yId = 1;
            foreach (var carModel in Cars)
            {
                foreach (var yearMileageModel in carModel.YearsMileage)
                {
                    yearMileageModel.Id = yId++;
                    yearMileageModel.CarId = carModel.Id;
                }
            }

            _dataModel.Cars = Cars;
        }

    }
}
