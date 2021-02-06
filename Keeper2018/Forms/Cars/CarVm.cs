using System;
using System.Collections.Generic;

namespace Keeper2018
{
    public class CarVm
    {
        public int Id { get; set; } //PK
        public int CarAccountId { get; set; }
        public string Title { get; set; }
        public int IssueYear { get; set; }
        public string Vin { get; set; }
        public string StateRegNumber { get; set; }

        public DateTime PurchaseDate { get; set; }
        public int PurchaseMileage { get; set; }
        public DateTime SaleDate { get; set; }
        public int SaleMileage { get; set; }

        public int SupposedSalePrice { get; set; }
        public string Comment { get; set; }
        

        public int MileageDifference => SaleMileage - PurchaseMileage;
        public int MileageAday => MileageDifference / (SaleDate - PurchaseDate).Days;
        public List<YearMileageVm> YearsMileage { get; set; } = new List<YearMileageVm>();

    }
}