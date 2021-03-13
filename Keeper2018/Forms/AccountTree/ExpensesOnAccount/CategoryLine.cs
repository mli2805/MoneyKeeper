namespace Keeper2018.ExpensesOnAccount
{
    public class CategoryLine
    {
        public string Category { get; set; }
        public decimal Total { get; set; }
        public string TotalStr => Total.ToString("#,0.##");
    }
}