﻿using System;
using System.Linq;
using System.Windows.Media;

namespace Keeper2018
{
    public static class MonthRatesDifferencesProvider
    {
        public static ListOfLines GetRatesDifference(this KeeperDataModel dataModel, DateTime startDate, DateTime finishMoment)
        {
            var result = new ListOfLines();
            var exchangeRatesLine = dataModel.GetExchangeRatesLine(startDate);
            double belkaStart = exchangeRatesLine.BynToUsd;

            var exchangeRatesLineFinish = dataModel.GetExchangeRatesLine(finishMoment);
            double belkaFinish = exchangeRatesLineFinish.BynToUsd;
            double belkaPercent = (belkaStart - belkaFinish) / belkaFinish * 100;

            var belkaName = finishMoment < new DateTime(2016, 7, 1) ? "Byr" : "Byn";
            var belkaWord = belkaFinish < belkaStart ? "вырос" : "упал";
            var belkaBrush = belkaFinish < belkaStart ? Brushes.Blue : Brushes.Red;
            var template = finishMoment < new DateTime(2016, 7, 1) ? "#,0" : "#,0.0000";

            var belka = $"      {belkaName} {belkaWord}: {belkaStart.ToString(template)} - {belkaFinish.ToString(template)}  ({belkaPercent:0.0}%)";
            result.Add(belka, belkaBrush);

            var euroStart = exchangeRatesLine.EurToUsd;
            var euroFinish = exchangeRatesLineFinish.EurToUsd;
            double euroPercent = (euroFinish - euroStart) / euroFinish * 100;
            var euroWord = euroFinish > euroStart ? "вырос" : "упал";
            var euroBrush = euroFinish > euroStart ? Brushes.Blue : Brushes.Red;
            var euro = $"      Euro {euroWord}: {euroStart:0.000} - {euroFinish:0.000}  ({euroPercent:0.0}%)";
            result.Add(euro, euroBrush);

            var rubStart = exchangeRatesLine.RubToUsd;
            var rubFinish = exchangeRatesLineFinish.RubToUsd;
            var rubPercent = (rubStart - rubFinish) / rubFinish * 100;
            var rubWord = rubFinish < rubStart ? "вырос" : "упал";
            var rubBrush = rubFinish < rubStart ? Brushes.Blue : Brushes.Red;
            var rub = $"      Rur {rubWord}: {rubStart:0.0} - {rubFinish:0.0}  ({rubPercent:0.0}%)";
            result.Add(rub, rubBrush);

            var gldStartByn = dataModel.MetalRates.LastOrDefault(m => m.Date <= startDate)?.Price ?? 0;
            var gldFinishByn = dataModel.MetalRates.LastOrDefault(m => m.Date <= finishMoment)?.Price ?? 0;

            var gldStart = gldStartByn / exchangeRatesLine.BynToUsd;
            var gldFinish = gldFinishByn / exchangeRatesLineFinish.BynToUsd;
            var gldPercent = (gldFinish - gldStart) / gldFinish * 100;
            var gldWord = gldFinish > gldStart ? "выросло" : "упало";
            var gldBrush = gldFinish > gldStart ? Brushes.Blue : Brushes.Red;
            var gld = $"      Gold {gldWord}: {gldStart:0.0} - {gldFinish:0.0}  ({gldPercent:0.0}%)";
            result.Add(gld, gldBrush);

            return result;
        }
    }
}