﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Keeper2018
{
    public static class OfficialRatesDownloader
    {
        public static async Task<MainCurrenciesRates> GetRatesForDate(DateTime date)
        {
            string uri = "http://www.nbrb.by/API/ExRates/Rates?onDate=" + $"{date:yyyy-M-d}" + "&Periodicity=0";
            var response = await GetAsync(uri);
            var nbList = (List<NbRbSiteRate>)JsonConvert.DeserializeObject(response, typeof(List<NbRbSiteRate>));
            var result = new MainCurrenciesRates();
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

        private static async Task<string> GetAsync(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync();
            Stream stream = response.GetResponseStream();
            if (stream == null) return "";
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class NbRbSiteRate
    {
        public int Cur_ID { get; set; }
        public DateTime Date { get; set; }
        public string Cur_Abbreviation { get; set; }
        public int Cur_Scale { get; set; }
        public string Cur_Name { get; set; }
        public double Cur_OfficialRate { get; set; }
    }
}
