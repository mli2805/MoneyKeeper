namespace Keeper2018
{
    public interface IIncomeForPeriod
    {
        void Fill(ListOfLines list);
        decimal GetTotal();
    }
}