﻿namespace Keeper2018
{
    public class YearMileageModel
    {
        public int Id { get; set; } //PK
        public int CarId { get; set; }
        public int YearNumber { get; set; }
        public int Year { get; set; }
        public int Odometer { get; set; }
        public int Mileage { get; set; }

    }
}