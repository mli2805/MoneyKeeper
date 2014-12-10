using System;

namespace Keeper.DomainModel
{
	public class DateProcentPoint
	{
		public DateTime Date { get; set; }
		public decimal Procent { get; set; }

		public DateProcentPoint() { }
		public DateProcentPoint(DateTime date, decimal procent)
		{
			Date = date;
			Procent = procent;
		}
	}
}