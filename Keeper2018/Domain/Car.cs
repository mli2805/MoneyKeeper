using System;
using System.Collections.Generic;

namespace Keeper2018
{
    public class Car
    {
        public int AccountId;
        public string Title;
        public int IssueYear;
        public string Vin;
        public string StateRegNumber;

        public DateTime Start;
        public int MileageStart;
        public DateTime Finish;
        public int MileageFinish;

        public List<CarYear> MileageYears;
    }

    public class CarYear
    {
        public int AccountId;
        public int YearCount;
        public int Mileage;
    }
}
