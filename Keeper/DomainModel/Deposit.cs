using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Windows.Media;

namespace Keeper.DomainModel
{
	public class Deposit
	{
		public Account Account { get; set; }
		public DateTime Start { get; set; }
		public DateTime Finish { get; set; }
		public CurrencyCodes MainCurrency { get; set; }
		public decimal StartAmount { get; set; }
		public decimal AdditionalAmounts { get; set; }
		public List<Transaction> Transactions { get; set; }
		public decimal CurrentBalance { get; set; }
		public DepositStates State { get; set; }
		public decimal DepositRate { get; set; }

		public decimal Profit { get; set; }
		public decimal Forecast { get; set; }

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
      if (Profit == 0) return 0;
      int startYear = Transactions.First().Timestamp.Year;
      int finishYear = Transactions.Last().Timestamp.AddDays(-1).Year;
      if (year < startYear || year > finishYear) return 0;
      if (startYear == finishYear) return Profit;
      int allDaysCount = (Transactions.Last().Timestamp.AddDays(-1) - Transactions.First().Timestamp).Days;
      if (year == startYear)
      {
        int startYearDaysCount = (new DateTime(startYear, 12, 31) - Transactions.First().Timestamp).Days;
        return Profit * startYearDaysCount / allDaysCount;
      }
      if (year == finishYear)
      {
        int finishYearDaysCount = (Transactions.Last().Timestamp.AddDays(-1) - new DateTime(finishYear, 1, 1)).Days;
        return Profit * finishYearDaysCount / allDaysCount;
      }
      int yearDaysCount = (new DateTime(year, 12, 31) - new DateTime(year, 1, 1)).Days;
      return Profit * yearDaysCount / allDaysCount;
    }

  }
}
