using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace Keeper.Models
{
  public class MonthAnalysisBlank : PropertyChangedBase
  {
    public ObservableCollection<string> BeforeList { get; set; }
    public ObservableCollection<string> BeforeListOnHands{ get; set; }
    public ObservableCollection<string> BeforeListOnDeposits{ get; set; }
    public ObservableCollection<string> IncomesToHandsList{ get; set; }
    public ObservableCollection<string> IncomesToDepositsList{ get; set; }
    public ObservableCollection<string> IncomesTotal{ get; set; }
    public ObservableCollection<string> ExpenseList{ get; set; }
    public ObservableCollection<string> LargeExpenseList{ get; set; }
    public ObservableCollection<string> AfterList{ get; set; }
    public ObservableCollection<string> AfterListOnHands{ get; set; }
    public ObservableCollection<string> AfterListOnDeposits{ get; set; }
    public ObservableCollection<string> ResultList{ get; set; }
    public ObservableCollection<string> DepositResultList { get; set; }
    public ObservableCollection<string> RatesList { get; set; }
//    public string ByrRates { get; set; }
//    public string EuroRates { get; set; }
    public ObservableCollection<string> ForecastListIncomes { get; set; }
    public ObservableCollection<string> ForecastListExpense{ get; set; }
    public ObservableCollection<string> ForecastListBalance{ get; set; }
  }
}