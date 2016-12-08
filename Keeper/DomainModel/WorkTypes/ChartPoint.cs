﻿namespace Keeper.DomainModel.WorkTypes
{
	public class ChartPoint
	{
		public string Subject { get; set; }
		public int Amount { get; set; }

		public ChartPoint() { }
		public ChartPoint(string subject, int amount)
		{
			Subject = subject;
			Amount = amount;
		}
	}
}