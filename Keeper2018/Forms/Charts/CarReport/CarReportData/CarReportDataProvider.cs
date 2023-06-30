using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KeeperDomain;

namespace Keeper2018
{
    public static class CarReportDataProvider
    {
        private static readonly string[] TagRussianNames =
            { "покупка-продажа", "государство", "авто ремонт", "ремонт ДТП", "регулярн обслуживание", "авто топливо", "авто прочее" };

        private static readonly string[] TagRussianNamesOther =
              { "покупка-продажа", "государство", "авто топливо", "авто прочее" };

        private static readonly string[] TagRussianNamesRepair =
             { "авто ремонт", "ремонт ДТП", "регулярн обслуживание" };

        private static readonly string[] TagEnglishNames =
            { "buy-sell", "state", "car repair", "accident repair", "expendables", "car fuel", "other stuff" };

        private static readonly string[] TagEnglishNamesOther =
                 { "buy-sell", "state", "car fuel", "other stuff" };

        // private static readonly string[] TagEnglishNamesRepair =
        //          { "car repair", "accident repair", "expendables" };


        public static CarReportData ExtractCarReportData(this KeeperDataModel dataModel, CarModel car, bool isByTags)
        {
            var isCurrentCar = dataModel.Cars.Last().Id == car.Id;

            var carReportData = dataModel.ExtractCarData(car.CarAccountId, isByTags);
            if (isCurrentCar)
            {
                carReportData.Tags[0].Table.Add(new CarReportTableRow()
                {
                    Date = DateTime.Today,
                    AmountInUsd = car.SupposedSalePrice,
                    AmountInCurrency = $"{car.SupposedSalePrice} usd",
                    Comment = "предполагаемая продажа",
                });
            }
            carReportData.FinishDate = carReportData.Tags[0].Table.Last().Date;
            return carReportData;
        }

        private static CarReportData ExtractCarData(this KeeperDataModel dataModel, int carAccountId, bool isByTags)
        {
            var result = new CarReportData();
            var carAccount = dataModel.AcMoDict[carAccountId];

            if (isByTags)
                for (int i = 0; i < TagRussianNames.Length; i++)
                {
                    // get Tag by name
                    var tag = (AccountItemModel)carAccount.Children.First(c => c.Name.Contains(TagRussianNames[i]));
                    // get rows for Tag
                    var rows = dataModel.GetTableForOneTag(tag);
                    result.Tags.Add(new CarReportTable(TagRussianNames[i], TagEnglishNames[i], rows));
                }
            else
            {
                for (int i = 0; i < TagRussianNamesOther.Length; i++)
                {
                    // get Tag by name
                    var tag = (AccountItemModel)carAccount.Children.First(c => c.Name.Contains(TagRussianNamesOther[i]));
                    // get rows for Tag
                    var rowsOther = dataModel.GetTableForOneTag(tag);
                    result.Tags.Add(new CarReportTable(TagRussianNamesOther[i], TagEnglishNamesOther[i], rowsOther));
                }

                var tagR0 = (AccountItemModel)carAccount.Children.First(c => c.Name.Contains(TagRussianNamesRepair[0]));
                var rowsRepair = dataModel.GetTableForOneTag(tagR0);
                var tagR1 = (AccountItemModel)carAccount.Children.First(c => c.Name.Contains(TagRussianNamesRepair[1]));
                rowsRepair.AddRange(dataModel.GetTableForOneTag(tagR1));
                var tagR2 = (AccountItemModel)carAccount.Children.First(c => c.Name.Contains(TagRussianNamesRepair[2]));
                rowsRepair.AddRange(dataModel.GetTableForOneTag(tagR2));

                result.Tags.Add(
                    new CarReportTable("обслуживание и ремонт", "expendables and repair",
                    rowsRepair.OrderBy(r => r.Date).ToList()));
            }

            result.StartDate = result.Tags[0].Table[0].Date;
            return result;
        }

        private static List<CarReportTableRow> GetTableForOneTag(this KeeperDataModel dataModel, AccountItemModel tag)
        {
            var rows = new List<CarReportTableRow>();
            foreach (var transaction in dataModel.Transactions.Values.OrderBy(t => t.Timestamp))
            {
                var balanceForTag = transaction.BalanceForTag(dataModel, tag.Id);
                if (balanceForTag == null) continue;
                var row = new CarReportTableRow
                {
                    Date = transaction.Timestamp,
                    AmountInCurrency = balanceForTag.ToString(),
                    Comment = transaction.Comment
                };

                if (Regex.IsMatch(transaction.Comment, @"\d{6} км, \w*"))
                {
                    var substring = transaction.Comment.Substring(0, 6);
                    if (int.TryParse(substring, out int mileage))
                    {
                        row.Mileage = mileage;
                        row.Comment = transaction.Comment.Substring(11);
                    }
                    
                }

                var moneyPair = balanceForTag.Currencies.First();
                row.AmountInUsd = moneyPair.Key == CurrencyCode.USD
                    ? moneyPair.Value
                    : dataModel.AmountInUsd(transaction.Timestamp, moneyPair.Key, moneyPair.Value);

                rows.Add(row);
            }
            return rows;
        }
    }
}