using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using KeeperDomain.Exchange;
using Newtonsoft.Json;

namespace Keeper2018
{
    public static class ExchangeRatesFetcher
    {
        public static async Task<List<ExchangeRates>> Get(int days)
        {
            var bankTitle = "BNB";
            var uri = $"http://192.168.96.19:11082/bali/get-some-last-days-for-bank?bankTitle={bankTitle}&days={days}";

            try
            {
                var response = await MyRequest.GetAsync(uri);
                var lines = (List<KomBankRatesLine>)JsonConvert.DeserializeObject(response, typeof(List<KomBankRatesLine>));
                return lines.Select(l => l.ToExchangeRates()).ToList();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }
    }
}
