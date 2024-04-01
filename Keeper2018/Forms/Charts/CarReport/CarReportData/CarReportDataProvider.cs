using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KeeperDomain;

namespace Keeper2018
{
    public static class CarReportDataProvider
    {
        private static readonly string[] CategoryRussianNames =
            { "покупка-продажа", "государство", "авто ремонт", "ремонт ДТП", "регулярн обслуживание", "авто топливо", "авто прочее" };

        private static readonly string[] CategoryRussianNamesOther =
              { "покупка-продажа", "государство", "авто топливо", "авто прочее" };

        private static readonly string[] CategoryRussianNamesRepair =
             { "авто ремонт", "ремонт ДТП", "регулярн обслуживание" };

        private static readonly string[] CategoryEnglishNames =
            { "buy-sell", "state", "car repair", "accident repair", "expendables", "car fuel", "other stuff" };

        private static readonly string[] CategoryEnglishNamesOther =
                 { "buy-sell", "state", "car fuel", "other stuff" };

        // private static readonly string[] CategoryEnglishNamesRepair =
        //          { "car repair", "accident repair", "expendables" };


        public static CarReportData ExtractCarReportData(this KeeperDataModel dataModel, CarModel car, bool isByCategories)
        {
            var isCurrentCar = dataModel.Cars.Last().Id == car.Id;

            var carReportData = dataModel.ExtractCarData(car.CarAccountId, isByCategories);
            if (isCurrentCar)
            {
                carReportData.Categories[0].Table.Add(new CarReportTableRow()
                {
                    Date = DateTime.Today,
                    AmountInUsd = car.SupposedSalePrice,
                    AmountInCurrency = $"{car.SupposedSalePrice} usd",
                    Comment = "предполагаемая продажа",
                });
            }
            carReportData.FinishDate = carReportData.Categories[0].Table.Last().Date;
            return carReportData;
        }

        private static CarReportData ExtractCarData(this KeeperDataModel dataModel, int carAccountId, bool isByCategories)
        {
            var result = new CarReportData();
            var carAccount = dataModel.AcMoDict[carAccountId];

            if (isByCategories)
                for (int i = 0; i < CategoryRussianNames.Length; i++)
                {
                    // get Category by name
                    var category = (AccountItemModel)carAccount.Children.First(c => c.Name.Contains(CategoryRussianNames[i]));
                    // get rows for Category
                    var rows = dataModel.GetTableForOneCategory(category);
                    result.Categories.Add(new CarReportTable(CategoryRussianNames[i], CategoryEnglishNames[i], rows));
                }
            else
            {
                for (int i = 0; i < CategoryRussianNamesOther.Length; i++)
                {
                    // get Category by name
                    var category = (AccountItemModel)carAccount.Children.First(c => c.Name.Contains(CategoryRussianNamesOther[i]));
                    // get rows for Category
                    var rowsOther = dataModel.GetTableForOneCategory(category);
                    result.Categories.Add(new CarReportTable(CategoryRussianNamesOther[i], CategoryEnglishNamesOther[i], rowsOther));
                }

                var tagR0 = (AccountItemModel)carAccount.Children.First(c => c.Name.Contains(CategoryRussianNamesRepair[0]));
                var rowsRepair = dataModel.GetTableForOneCategory(tagR0);
                var tagR1 = (AccountItemModel)carAccount.Children.First(c => c.Name.Contains(CategoryRussianNamesRepair[1]));
                rowsRepair.AddRange(dataModel.GetTableForOneCategory(tagR1));
                var tagR2 = (AccountItemModel)carAccount.Children.First(c => c.Name.Contains(CategoryRussianNamesRepair[2]));
                rowsRepair.AddRange(dataModel.GetTableForOneCategory(tagR2));

                result.Categories.Add(
                    new CarReportTable("обслуживание и ремонт", "expendables and repair",
                    rowsRepair.OrderBy(r => r.Date).ToList()));
            }

            result.StartDate = result.Categories[0].Table[0].Date;
            return result;
        }

        private static List<CarReportTableRow> GetTableForOneCategory(this KeeperDataModel dataModel, AccountItemModel category)
        {
            var rows = new List<CarReportTableRow>();
            foreach (var transaction in dataModel.Transactions.Values
                         .Where(t=>t.Category != null && t.Category.Id == category.Id)
                         .OrderBy(t => t.Timestamp))
            {
                var balanceForTag = transaction.BalanceForCategory();
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