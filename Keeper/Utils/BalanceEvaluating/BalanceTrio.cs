﻿using System;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;

namespace Keeper.Utils.BalanceEvaluating
{
  public class BalanceTrio
	{
		public Account MyAccount;
		public decimal Amount;
		public CurrencyCodes Currency;

		public new string ToString()
		{
			return String.Format("{0}  {1:#,0} {2}", MyAccount.Name, Amount, Currency.ToString().ToLower());
		}
	}
}