using Keeper.DomainModel.Enumes;

namespace Keeper.Utils.DepositProcessing
{
    public class DepositAmountTemplater
    {
        private readonly CurrencyCodes _currency;
        public DepositAmountTemplater(CurrencyCodes currency)
        {
            _currency = currency;
        }


        public string ForAmount() { return _currency == CurrencyCodes.BYR ? "{0:#,0}" : "{0:#,0.00}"; }
    }
}
