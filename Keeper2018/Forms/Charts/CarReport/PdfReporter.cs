using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public static class PdfReporter
    {
        private const int Scenic3Id = 716; // Scenic3
        private static readonly string[] TagRussians = 
            { "покупка-продажа", "государство", "авто ремонт", "ремонт регуляр", "авто топливо", "авто прочее" };
        private static readonly string[] TagEnglish = 
            { "buy-sell", "state", "car repair", "expendables", "car fuel", "other stuff" };

        public static CarReportData ExtractCarData(this KeeperDb db)
        {
            var result = new CarReportData();
            var carAccount = db.AcMoDict[Scenic3Id];
            for (int i = 0; i < TagRussians.Length; i++)
            {
                var tag = carAccount.Children.First(c => c.Name.Contains(TagRussians[i]));
                var rows = db.GetTableForTag(tag);
                result.Tags.Add(new CarTagData(TagRussians[i], TagEnglish[i], rows));
            }

            result.StartDate = result.Tags[0].Table[0].Date;

//            var sold = new PdfReportTableRow()
//            {
//                Date = new DateTime(2019, 4, 15),
//                AmountInUsd = 9200,
//                AmountInCurrency = "9200 usd",
//                Comment = "продажа",
//            };
//            result.Tags[0].Table.Add(sold);
//            result.FinishDate = sold.Date;

            result.FinishDate = DateTime.Today;
            return result;
        }

        private static List<PdfReportTableRow> GetTableForTag(this KeeperDb db, AccountModel tag)
        {
            var rows = new List<PdfReportTableRow>();
            foreach (var transaction in db.Bin.Transactions.Values)
            {
                var balanceForTag = transaction.BalanceForTag(db, tag.Id);
                if (balanceForTag == null) continue;
                var row = new PdfReportTableRow
                {
                    Date = transaction.Timestamp,
                    AmountInCurrency = balanceForTag.ToString(),
                    Comment = transaction.Comment
                };
                var moneyPair = balanceForTag.Currencies.First();
                row.AmountInUsd = moneyPair.Key == CurrencyCode.USD
                    ? moneyPair.Value
                    : db.AmountInUsd(transaction.Timestamp, moneyPair.Key, moneyPair.Value);

                rows.Add(row);
            }
            return rows;
        }

    }
}