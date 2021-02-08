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

        public Visibility IsLastCarVisibility => SelectedCar.Id == Cars.Last().Id
            ? Visibility.Visible : Visibility.Collapsed;

        public CarsViewModel(KeeperDataModel dataModel, IWindowManager windowManager, FuelViewModel fuelViewModel)
        {
            _dataModel = dataModel;
            _windowManager = windowManager;
            _fuelViewModel = fuelViewModel;
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
            if (SelectedCar.Id < 3) return;
            var provider = new CarReportProvider(_dataModel);
            var document = provider.CreateCarReport(SelectedCar.Id);

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
        public void Close() { TryClose(); }

    }
}
