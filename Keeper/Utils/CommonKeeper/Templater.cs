using Keeper.DomainModel;

namespace Keeper.Utils.CommonKeeper
{
    public class Templater
    {
        private readonly CurrencyCodes _currency;
        public Templater(CurrencyCodes currency)
        {
            _currency = currency;
        }


        public string ForAmount() { return _currency == CurrencyCodes.BYR ? "{0:#,0}" : "{0:#,0.00}"; }
        public string ForAmount(int position) { return _currency == CurrencyCodes.BYR ? "{"+position+":#,0}" : "{"+position+"0:#,0.00}"; }
    }
}
