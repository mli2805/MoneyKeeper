using KeeperDomain;

namespace Keeper2018
{
    public class YearMileageModel
    {
        public int Id { get; set; } //PK
        public int CarId { get; set; }
        public int YearNumber { get; set; } // ordinal
        // public int Year { get; set; }
        public int Odometer { get; set; }

        //вычисляемые поля
        public Period Period { get; set; }
        public string FromTo => Period.ToStringD();
        public int Mileage { get; set; }
        public decimal YearAmount { get; set; }
        public decimal DayAmount { get; set; }

    }
}