using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public class ExpenseTran
    {
        public DateTime Timestamp { get; set; }
        public int Receipt { get; set; }
        public decimal Amount { get; set; }
        public CurrencyCode Currency { get; set; }
        public List<int> Tags { get; set; }
        public string Comment { get; set; }


        public ExpenseTran(TransactionModel tr)
        {
            Timestamp = tr.Timestamp;
            Receipt = tr.Receipt;
            Amount = tr.Amount;
            Currency = tr.Currency;
            Tags = tr.Tags.Select(t=>t.Id).ToList();
            Comment = tr.Comment;
        }

        public string ToString(KeeperDataModel dataModel)
        {
            return Timestamp.ToString("dd/MM/yyyy HH:mm") + " ; " +
                   Amount.ToString(new CultureInfo("en-US")) + " ; " + Currency + " ; " +
                   TagsToString(dataModel) + " ; " + Comment;
        }

        private string TagsToString(KeeperDataModel dataModel)
        {
            if (Tags == null || Tags.Count == 0) return " ";


            string result = "";
            foreach (var t in Tags)
            {
                var acc = dataModel.AcMoDict[t];
                if (Receipt > 0 && acc.Is(179))
                    return acc.Name;
                result = result + acc.Name + " | ";
            }
            result = result.Substring(0, result.Length - 3);
            return result;
        }
    }
}