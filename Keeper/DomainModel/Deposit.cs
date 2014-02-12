using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;

namespace Keeper.DomainModel
{
  public enum DepositOperations
  {
    Явнес,
    Проценты,
    Расход,
  }

  public class DepositTransaction
  {
    public DateTime Timestamp { get; set; }
    public DepositOperations TransactionType { get; set; }
    public Decimal Amount { get; set; }
    public CurrencyCodes Currency { get; set; }
    public Decimal AmountInUsd { get; set; }
    public string Comment { get; set; }
  }

  public class DepositRateLine
  {
    public decimal AmountFrom { get; set; }
    public decimal AmountTo { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public decimal Rate { get; set; }
  }

	public class Deposit : PropertyChangedBase
	{
		public Account Account { get; set; }
    public Account Bank { get; set; }
    public string Title { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime FinishDate { get; set; }
		public CurrencyCodes Currency { get; set; }
    public DepositStates State { get; set; }

    public decimal DepositRate { get; set; }
    public List<DepositRateLine> DepositRateLines { get; set; }

    // дальше вычислимое , не должно храниться?
    public List<DepositTransaction> Traffic { get; set; }
    public decimal TotalMyIns { get; set; }
    public decimal TotalPercent { get; set; }
    public decimal TotalMyOuts { get; set; }
    public decimal CurrentBalance { get { return TotalMyIns + TotalPercent - TotalMyOuts; } }
    public decimal CurrentProfit { get; set; }

    public decimal EstimatedProcents { get; set; }
    public decimal EstimatedProfitInUsd { get; set; }

		public Brush FontColor
		{
			get
			{
				if (State == DepositStates.Закрыт) return Brushes.Gray;
				if (State == DepositStates.Просрочен) return Brushes.Red;
				return Brushes.Blue;
			}
		}

    public decimal GetProfitForYear(int year)
    {
      if (CurrentProfit == 0) return 0;
      int startYear = Traffic.First().Timestamp.Year;
      int finishYear = Traffic.Last().Timestamp.AddDays(-1).Year;
      if (year < startYear || year > finishYear) return 0;
      if (startYear == finishYear) return CurrentProfit;
      int allDaysCount = (Traffic.Last().Timestamp.AddDays(-1) - Traffic.First().Timestamp).Days;
      if (year == startYear)
      {
        int startYearDaysCount = (new DateTime(startYear, 12, 31) - Traffic.First().Timestamp).Days;
        return CurrentProfit * startYearDaysCount / allDaysCount;
      }
      if (year == finishYear)
      {
        int finishYearDaysCount = (Traffic.Last().Timestamp.AddDays(-1) - new DateTime(finishYear, 1, 1)).Days;
        return CurrentProfit * finishYearDaysCount / allDaysCount;
      }
      int yearDaysCount = (new DateTime(year, 12, 31) - new DateTime(year, 1, 1)).Days;
      return CurrentProfit * yearDaysCount / allDaysCount;
    }

  }
}
