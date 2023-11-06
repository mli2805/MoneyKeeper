using System;
using System.Globalization;

namespace KeeperDomain
{
    [Serializable]
    public class Car : IDumpable, IParsable<Car>
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

        public Car FromString(string s)
        {
            var substrings = s.Split(';');

            Id = int.Parse(substrings[0].Trim());
            CarAccountId = int.Parse(substrings[1].Trim());
            Title = substrings[2].Trim();
            IssueYear = int.Parse(substrings[3].Trim());
            Vin = substrings[4].Trim();
            StateRegNumber = substrings[5].Trim();

            PurchaseDate = DateTime.ParseExact(substrings[6].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            PurchaseMileage = int.Parse(substrings[7].Trim());
            SaleDate = DateTime.ParseExact(substrings[8].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            SaleMileage = int.Parse(substrings[9].Trim());

            SupposedSalePrice = int.Parse(substrings[10].Trim());
            Comment = substrings[11].Trim();
            return this;
        }
    }
}
