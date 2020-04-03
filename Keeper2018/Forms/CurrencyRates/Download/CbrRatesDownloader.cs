using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;

namespace Keeper2018
{
    public static class CbrRatesDownloader
    {
//        public static async Task<double> GetRateForDate(DateTime date)
//        {
//            try
//            {
//                string uri = "http://www.cbr.ru/currency_base/daily/?date_req=" + $"{date:dd.MM.yyyy}";
//                var response = await MyRequest.GetAsync(uri);
//
//                var index = response.IndexOf("USD", StringComparison.CurrentCulture);
//                var rateStr = response.Substring(index + 50, 27);
//                return double.TryParse(rateStr, NumberStyles.Any, new CultureInfo("ru-RU"), out double rate) ? rate : 0;
//
//            }
//            catch (Exception e)
//            {
//                return 0;
//            }
//        }

        public static async Task<double> GetRateForDateFromXml(DateTime date)
        {
            try
            {
                // currency code could be obtained from http://www.cbr.ru/scripts/XML_val.asp?d=0
                string usdCode = "R01235";
                string uri =
                    $"http://www.cbr.ru/scripts/XML_dynamic.asp?date_req1={date:dd.MM.yyyy}&date_req2={date:dd.MM.yyyy}&VAL_NM_RQ={usdCode}";
                var response = await MyRequest.GetAsync(uri);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(response);
                var json = JsonConvert.SerializeXmlNode(doc)
                    .Replace("@", "")
                    .Replace("?", "");
                var cbrfRate = (RootObject)JsonConvert.DeserializeObject(json, typeof(RootObject));
                if (cbrfRate.ValCurs.Record == null)
                    return 0;
                return double.Parse(cbrfRate.ValCurs.Record.Value, NumberStyles.Any);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return 0;
            }
        }
    }

    public class Xml
    {
        public string version { get; set; }
        public string encoding { get; set; }
    }

    public class Record
    {
        public string Date { get; set; }
        public string Id { get; set; }
        public string Nominal { get; set; }
        public string Value { get; set; }
    }

    public class ValCurs
    {
        public string ID { get; set; }
        public string DateRange1 { get; set; }
        public string DateRange2 { get; set; }
        public string name { get; set; }
        public Record Record { get; set; }
    }

    public class RootObject
    {
        public Xml Xml { get; set; }
        public ValCurs ValCurs { get; set; }
    }
}