﻿using System;
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

        public void AddNewCar()
        {

        }

        public void Fuelling()
        {
            _fuelViewModel.Initialize();
            _windowManager.ShowWindow(_fuelViewModel);
        }

        public void ShowCarReportByTags()
        {
            ShowCarReport(true);
        }

        public void ShowCarReportUnited()
        {
            ShowCarReport(false);
        }

        private void ShowCarReport(bool isByTags)
        {
            if (SelectedCar.Id < 3) return;
            var document = _dataModel.CreateCarReport(SelectedCar.Id, isByTags);

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
