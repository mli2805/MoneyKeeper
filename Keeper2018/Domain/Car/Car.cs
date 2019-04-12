using System;

namespace Keeper2018
{
    [Serializable]
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

        public int SupposedSale;
        public string Comment;

        public int[] YearMileages;
    }
}
