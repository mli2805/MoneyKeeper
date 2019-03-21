using System.Collections.Generic;

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

        public string Start => $"{_car.Start:d MMMM yyyy}" ;
        public int MileageStart => _car.MileageStart;

        public string Finish => $"{_car.Finish:d MMMM yyyy}" ;
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
        public List<CarYear> MileageYears => _car.MileageYears;

        public CarVm(Car car)
        {
            _car = car;
        }
    }
}