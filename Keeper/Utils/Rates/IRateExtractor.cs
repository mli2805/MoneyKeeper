using System;

using Keeper.DomainModel;

namespace Keeper.Utils.Rates
{
	public interface IRateExtractor {
		double GetRate(CurrencyCodes currency, DateTime day);
		double GetLastRate(CurrencyCodes currency);
		double GetRateThisDayOrBefore(CurrencyCodes currency, DateTime day);
		decimal GetUsdEquivalent(decimal amount, CurrencyCodes currency, DateTime timestamp);
		string GetUsdEquivalentString(decimal amount, CurrencyCodes currency, DateTime timestamp);
		string GetUsdEquivalentString(decimal amount, CurrencyCodes currency, DateTime timestamp, out decimal amountInUsd);
	}
}