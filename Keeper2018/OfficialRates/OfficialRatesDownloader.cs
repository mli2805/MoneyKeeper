using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Keeper2018
{
    public static class OfficialRatesDownloader
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        public static async Task<int> GetRatesForDate(DateTime date)
        {
            var r = new NbRbSiteRate() { Cur_ID = 298, Date = DateTime.Today, Cur_Abbreviation = "RUB", Cur_Scale = 100, Cur_Name = "rosroror", Cur_OfficialRate = 3.13251 };
            var li = new List<NbRbSiteRate>(){r,r,r};
            var str = JsonConvert.SerializeObject(li, JsonSerializerSettings);
            File.WriteAllText(@"c:\temp\requestJ.txt", str);

            var o = JsonConvert.DeserializeObject(str, JsonSerializerSettings);
            var dd = (NbRbSiteRate)o;

            //string uri = "http://www.nbrb.by/API/ExRates/Rates?onDate="+$"{date:yyyy-M-d}"+"&Periodicity=0";
            string uri = "http://www.nbrb.by/API/ExRates/Rates/" + "298" + "?onDate=" + "2016-7-5";
            var result = await GetAsync(uri);
            var json = "\"$type\":\"Keeper2018.NbRbSiteRate, Keeper2018\",";
            var rrrr = "{" + json + result.Substring(1);
            File.WriteAllText(@"c:\temp\request1.txt", rrrr);
            var rate = JsonConvert.DeserializeObject(result, typeof(NbRbSiteRate));
            return 1;
        }

        public static async Task<string> GetAsync(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())

            using (Stream stream = response.GetResponseStream())

            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }

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
