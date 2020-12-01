﻿using System;
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
            _db.Bin.Cars.Last().SaleDate = DateTime.Today;

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

    }
}
