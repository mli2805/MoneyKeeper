using System;

namespace KeeperDomain
{
    [Serializable]
    public class Car
    {
        public int Id { get; set; } //PK
        public int AccountId { get; set; }
        public string Title { get; set; }
        public int IssueYear { get; set; }
        public string Vin { get; set; }
        public string StateRegNumber { get; set; }

        public DateTime Start { get; set; }
        public int MileageStart { get; set; }
        public DateTime Finish { get; set; }
        public int MileageFinish { get; set; }

        public int SupposedSale { get; set; }
        public string Comment { get; set; }

        public YearMileage[] YearMileages { get; set; }
    }

    [Serializable]
    public class YearMileage
    {
        public int Id { get; set; } //PK
        public int CarId { get; set; }
        public int YearNumber { get; set; }
        public int Mileage { get; set; }
    }
}
