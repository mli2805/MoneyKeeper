using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;
using KeeperDomain;
using Newtonsoft.Json;
using Serilog;

namespace Keeper2018
{
    public static class CbrRatesDownloader
    {
        // XML API
        public static async Task<OneRate> GetRateForDateFromXml(DateTime date)
        {
            try
            {
                // currency CODE could be obtained from http://www.cbr.ru/scripts/XML_val.asp?d=0
                string usdCode = "R01235";

                // запрос создается для диапазона дат, если нужен один день - начало и конец совпадают
                string uri =
                    $"http://www.cbr.ru/scripts/XML_dynamic.asp?date_req1={date:dd.MM.yyyy}&date_req2={date:dd.MM.yyyy}&VAL_NM_RQ={usdCode}";
                var response = await MyRequest.GetAsync(uri);
                // ответ есть для любой даты (диапазона), но для некоторых дней внутри нету поля Record
                // например, в по итогам торгов в пятницу устанавливается курс на субботу, 
                //           поэтому запрос курса субботы вернёт результат с курсом,
                //           далее запросы курса воскресенья и понедельника придут пустые,
                //           при этом на сайте цб рф (не api) будет написано, что курс установлен С субботы, т.е. и на вск и на пнд
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(response);
                var json = JsonConvert.SerializeXmlNode(doc)
                    .Replace("@", "")
                    .Replace("?", "");
                var cbrfRate = (RootObject)JsonConvert.DeserializeObject(json, typeof(RootObject));
                if (cbrfRate.ValCurs.Record == null)
                    return null;
                var value = double.Parse(cbrfRate.ValCurs.Record.Value, NumberStyles.Any);
                var nominal = int.Parse(cbrfRate.ValCurs.Record.Nominal);
                return new OneRate() { Unit = nominal, Value = value };
            }
            catch (Exception e)
            {
                Log.Error(e, "CbrRatesDownloader::GetRateForDateFromXml");
                return null;
            }
        }
    }

    public class Xml
    {
        public string Version { get; set; }
        public string Encoding { get; set; }
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
        public string Id { get; set; }
        public string DateRange1 { get; set; }
        public string DateRange2 { get; set; }
        public string Name { get; set; }
        public Record Record { get; set; }
    }

    public class RootObject
    {
        public Xml Xml { get; set; }
        public ValCurs ValCurs { get; set; }
    }
}