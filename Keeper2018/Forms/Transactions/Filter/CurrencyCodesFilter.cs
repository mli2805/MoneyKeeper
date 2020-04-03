using KeeperDomain;

namespace Keeper2018
{
    public class CurrencyCodesFilter
    {
        public bool IsOn { get; set; }
        public CurrencyCode Currency { get; set; }

        /// <summary>
        /// таким конструктором создается ВЫключенный фильтр
        /// ему не нужна валюта, он пропускает все валюты
        /// </summary>
        public CurrencyCodesFilter() { IsOn = false; }

        /// <summary>
        /// а такой фильтр пропускает только "свою" валюту
        /// </summary>
        /// <param name="currency"></param>
        public CurrencyCodesFilter(CurrencyCode currency)
        {
            IsOn = true;
            Currency = currency;
        }

        public override string ToString()
        {
            return IsOn ? Currency.ToString() : "<no filter>";
        }
    }
}