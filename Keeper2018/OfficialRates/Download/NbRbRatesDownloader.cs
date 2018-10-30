using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Keeper2018
{
    public static class NbRbRatesDownloader
    {
        public static async Task<NbRbRates> GetRatesForDate(DateTime date)
        {
            string uri = "http://www.nbrb.by/API/ExRates/Rates?onDate=" + $"{date:yyyy-M-d}" + "&Periodicity=0";
            var response = await MyRequest.GetAsync(uri);
            var nbList = (List<NbRbSiteRate>)JsonConvert.DeserializeObject(response, typeof(List<NbRbSiteRate>));
            if (nbList.Count == 0) return null; 
            var result = new NbRbRates();
            var usdRate = nbList.First(c => c.Cur_Abbreviation == "USD");
            result.Usd.Value = usdRate.Cur_OfficialRate;
            result.Usd.Unit = usdRate.Cur_Scale;
            var euroRate = nbList.First(c => c.Cur_Abbreviation == "EUR");
            result.Euro.Value = euroRate.Cur_OfficialRate;
            result.Euro.Unit = euroRate.Cur_Scale;
            var rubRate = nbList.First(c => c.Cur_Abbreviation == "RUB");
            result.Rur.Value = rubRate.Cur_OfficialRate;
            result.Rur.Unit = rubRate.Cur_Scale;
            return result;
        }
    }
}
