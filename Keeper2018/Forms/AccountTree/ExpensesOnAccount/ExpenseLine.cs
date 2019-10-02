namespace Keeper2018.ExpensesOnAccount
{
    public class ExpenseLine
    {
        public string Category { get; set; }
        public decimal Total { get; set; }
        public string TotalStr => Total.ToString("#,0.##");
    }
}