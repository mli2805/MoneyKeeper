using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public class YearIncome : IIncomeForPeriod 
    {
        public Dictionary<AccountItemModel, decimal> Employers = new Dictionary<AccountItemModel, decimal>();
        public Dictionary<CurrencyCode, decimal> DepoByCurrency = new Dictionary<CurrencyCode, decimal>();
        public Dictionary<AccountItemModel, decimal> Cards = new Dictionary<AccountItemModel, decimal>();
        public Dictionary<AccountItemModel, decimal> Rests = new Dictionary<AccountItemModel, decimal>();

        public void Fill(ListOfLines list)
        {
            list.InsertYearSalary(Employers)
                .InsertYearDepositIncome(DepoByCurrency)
                .InsertYearMoneyback(Cards)
                .InsertYearRest(Rests);
        }

        public decimal GetTotal()
        {
            var depoTotal = DepoByCurrency.Values.Sum();
            var salaryTotal = Employers.Values.Sum();
            var moneyBackTotal = Cards.Values.Sum();
            var restTotal = Rests.Values.Sum();
            return depoTotal + salaryTotal + moneyBackTotal + restTotal;
        }
    }
}