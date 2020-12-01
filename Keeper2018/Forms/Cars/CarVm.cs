using System.Collections.Generic;
using KeeperDomain;

namespace Keeper2018
{
    public class CarVm
    {
        private Car _car;

        public int AccountId => _car.CarAccountId;
        public string Title => _car.Title;
        public int IssueYear => _car.IssueYear;
        public string Vin => _car.Vin;
        public string StateRegNumber => _car.StateRegNumber;

        public string Start => $"{_car.PurchaseDate:d MMMM yyyy}";
        public int MileageStart => _car.PurchaseMileage;

        public string Finish => $"{_car.SaleDate:d MMMM yyyy}";
        public int MileageFinish
        {
            get => _car.SaleMileage;
            set => _car.SaleMileage = value;
        }

        public int SupposedSale
        {
            get => _car.SupposedSalePrice;
            set => _car.SupposedSalePrice = value;
        }

        public string Comment => _car.Comment;

        public int MileageDifference => _car.SaleMileage - _car.PurchaseMileage;
        public int MileageAday => MileageDifference / (_car.SaleDate - _car.PurchaseDate).Days;
        public List<CarYearVm> YearsMileage { get; set; } = new List<CarYearVm>();

        public CarVm(Car car)
        {
            _car = car;
            for (int i = 0; i < car.YearMileages.Length; i++)
            {
                YearsMileage.Add(new CarYearVm()
                {
                    YearCount = i + 1,
                    Mileage = car.YearMileages[i].Mileage,
                    YearMileage = i == 0
                                  ? car.YearMileages[0].Mileage - car.PurchaseMileage
                                  : car.YearMileages[i].Mileage - car.YearMileages[i - 1].Mileage,
                });
            }
        }
    }
}