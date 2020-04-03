using System.Collections.Generic;
using KeeperDomain;

namespace Keeper2018
{
    public class CarVm
    {
        private Car _car;

        public int AccountId => _car.AccountId;
        public string Title => _car.Title;
        public int IssueYear => _car.IssueYear;
        public string Vin => _car.Vin;
        public string StateRegNumber => _car.StateRegNumber;

        public string Start => $"{_car.Start:d MMMM yyyy}";
        public int MileageStart => _car.MileageStart;

        public string Finish => $"{_car.Finish:d MMMM yyyy}";
        public int MileageFinish
        {
            get => _car.MileageFinish;
            set => _car.MileageFinish = value;
        }

        public int SupposedSale
        {
            get => _car.SupposedSale;
            set => _car.SupposedSale = value;
        }

        public string Comment => _car.Comment;

        public int MileageDifference => _car.MileageFinish - _car.MileageStart;
        public int MileageAday => MileageDifference / (_car.Finish - _car.Start).Days;
        public List<CarYearVm> YearsMileage { get; set; } = new List<CarYearVm>();

        public CarVm(Car car)
        {
            _car = car;
            for (int i = 0; i < car.YearMileages.Length; i++)
            {
                YearsMileage.Add(new CarYearVm()
                {
                    YearCount = i + 1,
                    Mileage = car.YearMileages[i],
                    YearMileage = i == 0
                                  ? car.YearMileages[0] - car.MileageStart
                                  : car.YearMileages[i] - car.YearMileages[i - 1],
                });
            }
        }
    }
}