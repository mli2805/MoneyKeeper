using System;

namespace Keeper2018
{
    [Serializable]
    public class CarYear
    {
        public int AccountId;
        public int YearCount { get; set; }
        public int Mileage { get; set; }
    }
}