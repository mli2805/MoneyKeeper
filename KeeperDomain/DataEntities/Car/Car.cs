using System;

namespace KeeperDomain
{
    [Serializable]
    public class Car : IDumpable
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

        public string Dump()
        {
            return Id + " ; " + CarAccountId + " ; " + Title + " ; " + IssueYear + " ; " + Vin + " ; " + StateRegNumber + " ; " 
                   + PurchaseDate.ToString("dd/MM/yyyy") + " ; " + PurchaseMileage + " ; "
                   + SaleDate.ToString("dd/MM/yyyy") + " ; " + SaleMileage + " ; " + SupposedSalePrice + " ; "
                   + Comment;
        }
    }
}
