using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Keeper2018
{
    public static class CbrRatesDownloader
    {
        public static async Task<double> GetRateForDate(DateTime date)
        {
            string uri = "http://www.cbr.ru/currency_base/daily/?date_req=" + $"{date:dd.MM.yyyy}";
            var response = await MyRequest.GetAsync(uri);

            var index = response.IndexOf("USD", StringComparison.CurrentCulture);
            var rateStr = response.Substring(index + 65, 7);
            return double.TryParse(rateStr, NumberStyles.Any, new CultureInfo("ru-RU"), out double rate) ? rate : 0;
        }
    }
}